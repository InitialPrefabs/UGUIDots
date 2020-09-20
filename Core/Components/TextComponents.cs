using System;
using System.Runtime.CompilerServices;
using TMPro;
using UGUIDOTS.Transforms;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.TextCore;

namespace UGUIDOTS {

    /// <summary>
    /// Stores a buffer of character values
    /// </summary>
    public struct CharElement : IBufferElementData {
        public char Value;

        public static implicit operator CharElement(char value) => new CharElement { Value = value };
        public static implicit operator char(CharElement value) => value.Value;
    }

    /// <summary>
    /// Stores glyph metric information to help generate the vertices required for the mesh.
    /// </summary>
    public struct GlyphElement : IBufferElementData {
        public ushort Unicode;
        public float Advance;
        public float2 Bearings;
        public float2 Size;
        public float Scale;

        /// <summary>
        /// Should be considered read only...use the extension functions to grab the UV coords. These
        /// values are not normalized.
        /// </summary>
        public float4 RawUV;
        public FontStyles Style;
    }

    /// <summary>
    /// Stores the unique identifier for the text component's font.
    /// </summary>
    public struct TextFontID : IComponentData, IEquatable<TextFontID> {
        public int Value;

        public bool Equals(TextFontID other) {
            return other.Value == Value;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    /// <summary>
    /// Stores stylizations of the text component.
    /// </summary>
    public struct TextOptions : IComponentData {
        public ushort Size;
        public FontStyles Style;
        public AnchoredState Alignment;
    }

    /// <summary>
    /// Stores the Font's unique identifier - this should typically be used on the "font" being converted.
    /// </summary>
    public struct FontID : IComponentData, IEquatable<FontID> {
        public int Value;

        public bool Equals(FontID other) {
            return other.Value == Value;
        }

        public override int GetHashCode() {
            return Value.GetHashCode();
        }
    }

    public static class GlyphExtensions {

        /// <summary>
        /// Normalizes the uvs based on the atlas size and some kind of style padding.
        /// </summary>
        /// <param name="uvMinMax">A vector 4 value where xy are the mins and zw are the maxes.</param>
        /// <param name="stylePadding">Pixel based dimension to shift the uvs.</param>
        /// <param name="atlasSize">The max size of the atlas.</param>
        /// <returns>A float2x4 matrix is returned with the order: bottom left, top left, top right, bottom right.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float2x4 NormalizeAdjustedUV(this in float4 uvMinMax, float stylePadding,
            float2 atlasSize) {
            var minX = uvMinMax.x - stylePadding;
            var minY = uvMinMax.y - stylePadding;
            var maxX = uvMinMax.z + stylePadding;
            var maxY = uvMinMax.w + stylePadding;

            // BL, TL, TR, BR -> Order of the float2x4
            return new float2x4(
                new float2(minX, minY) / atlasSize,
                new float2(minX, maxY) / atlasSize,
                new float2(maxX, maxY) / atlasSize,
                new float2(maxX, minY) / atlasSize
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetGlyph(this in NativeArray<GlyphElement> glyphs, in char c, out GlyphElement glyph) {
            for (int i = 0; i < glyphs.Length; i++) {
                var current = glyphs[i];

                if (current.Unicode == (ushort)c) {
                    glyph = current;
                    return true;
                }
            }

            glyph = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetGlyph(this in DynamicBuffer<GlyphElement> glyphs, in char c, out GlyphElement glyph) {
            var glyphArray = glyphs.AsNativeArray();
            return glyphArray.TryGetGlyph(in c, out glyph);
        }
    }

    /// <summary>
    /// A copy of the FaceInfo struct generated by UnityEngine's TextCore.LowLevel;
    /// </summary>
    public struct FontFaceInfo : IComponentData {

        public float AscentLine;
        public float BaseLine;
        public float CapLine;
        public float DescentLine;
        public float LineHeight;
        public float MeanLine;
        public float PointSize;
        public float Scale;
        public float StrikeThroughOffset;
        public float StrikeThroughThickness;
        public float SubscriptSize;
        public float SubscriptOffset;
        public float SuperscriptSize;
        public float SuperscriptOffset;
        public float TabWidth;
        public float UnderlineOffset;
        public float UnderlineThickness;
        public float2 NormalStyle;
        public float2 BoldStyle;
        public int2 AtlasSize;
        public FixedString32 FamilyName;

        public static implicit operator FontFaceInfo(in FaceInfo info) {
            return new FontFaceInfo {
                AscentLine             = info.ascentLine,
                BaseLine               = info.baseline,
                CapLine                = info.capLine,
                DescentLine            = info.descentLine,
                FamilyName             = info.familyName,
                MeanLine               = info.meanLine,
                PointSize              = info.pointSize,
                Scale                  = info.scale,
                StrikeThroughThickness = info.strikethroughThickness,
                StrikeThroughOffset    = info.strikethroughThickness,
                SubscriptSize          = info.subscriptSize,
                SubscriptOffset        = info.subscriptOffset,
                SuperscriptSize        = info.superscriptSize,
                SuperscriptOffset      = info.superscriptOffset,
                TabWidth               = info.tabWidth,
                UnderlineOffset        = info.underlineOffset
            };
        }
    }
}
