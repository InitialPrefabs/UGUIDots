﻿using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UGUIDots;
using UGUIDots.Render;
using UnityEngine;
using UnityEngine.TextCore;
using Unity.Mathematics;
using UnityEngine.Rendering;

public class TextGeneration : MonoBehaviour {

    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData {
        public float3 Position;
        public float3 Normal;
        public float2 UV;
    }

    public OrthographicRenderFeature Feature;
    public TMP_FontAsset FontAsset;
    public string Text;
    public Vector2 size;
    public Font FontToUse;
    public float Spacing = 1.0f;
    public Material Material;

    List<VertexData> vertexInfo;
    List<uint> indices;
    Dictionary<uint, Glyph> glyphLookUp;
    MaterialPropertyBlock block;
    Mesh mesh;
    string _internal;

    void Start() {
        block = new MaterialPropertyBlock();

        glyphLookUp = FontAsset.glyphLookupTable;
        // block.SetColor(ShaderIDConstants.Color, Color.green);

        // Material = Canvas.GetDefaultCanvasMaterial();
        mesh = new Mesh();

        vertexInfo = new List<VertexData>();
        indices = new List<uint>();
    }

    void Update() {
        if (Text.Length == 0) {
            mesh.Clear();
            return;
        }

        if (Text != _internal) {
            mesh.Clear();
            vertexInfo.Clear();
            indices.Clear();
            RenderTextQuads(Screen.width / 2, Screen.height / 2, 1);
            _internal = Text;
        }

        var m = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.identity, Vector3.one);
        Feature.Pass.InstructionQueue.Enqueue((mesh, Material, m, block));
    }

    void RenderTextQuads(float x, float y, float scale)
    {
        for (int i = 0; i < Text.Length; i++)
        {
            var c = Text[i];
            FontToUse.GetCharacterInfo(c, out CharacterInfo glyph);

            var xPos = x + glyph.BearingX() * scale;
            var yPos = y - (glyph.Height() - glyph.BearingY(0)) * scale;

            var width = glyph.Width() * scale;
            var height = glyph.Height() * scale;

            var BL = new Vector3(xPos, yPos);
            var TL = new Vector3(xPos, yPos + height);
            var TR = new Vector3(xPos + width, yPos + height);
            var BR = new Vector3(xPos + width, yPos);

            vertexInfo.Add(new VertexData {
                Position = BL,
                Normal   = Vector3.right,
                UV       = glyph.uvBottomLeft
            });
            vertexInfo.Add(new VertexData {
                Position = TL,
                Normal   = Vector3.right,
                UV       = glyph.uvTopLeft
            });
            vertexInfo.Add(new VertexData {
                Position = TR,
                Normal   = Vector3.right,
                UV       = glyph.uvTopRight,
            });
            vertexInfo.Add(new VertexData {
                Position = BR,
                Normal   = Vector3.right,
                UV       = glyph.uvBottomRight
            });

            var baseIndex = (uint)vertexInfo.Count - 4;

            indices.AddRange(new uint[] { 
                baseIndex, baseIndex + 1, baseIndex + 2,
                baseIndex, baseIndex + 2, baseIndex + 3
            });

            x += (glyph.Advance() * Spacing) * scale;

            /*
            Gizmos.DrawLine(BL, TL);
            Gizmos.DrawLine(TL, TR);
            Gizmos.DrawLine(TR, BR);
            Gizmos.DrawLine(BL, BR);
            */
        }

        mesh.SetVertexBufferParams(vertexInfo.Count, 
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2));

        mesh.SetVertexBufferData(vertexInfo, 0, 0, vertexInfo.Count, 0);

        mesh.SetIndexBufferParams(indices.Count, IndexFormat.UInt32);
        mesh.SetIndexBufferData(indices, 0, 0, indices.Count);

        mesh.subMeshCount = 1;
        mesh.SetSubMesh(0, new SubMeshDescriptor(0, indices.Count, MeshTopology.Triangles));
        mesh.UploadMeshData(false);
    }
}
