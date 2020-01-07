using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;

namespace UGUIDots.Render.Systems {

    public struct RenderPair : IComparer<RenderPair>, IEquatable<RenderPair> {
        public Entity Root;
        public int ID;

        public int Compare(RenderPair x, RenderPair y) {
            return x.ID.CompareTo(y.ID);
        }

        public bool Equals(RenderPair other) {
            return other.Root == Root && other.ID == ID;
        }
        public override int GetHashCode() {
            return Root.GetHashCode() ^ ID.GetHashCode();
        }
    }

    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(RenderBatchSystem))]
    public class RenderSortSystem : ComponentSystem {

        private class RenderGroupComparer : IComparer<RenderPair> {
            public int Compare(RenderPair x, RenderPair y) {
                return x.ID.CompareTo(y.ID);
            }
        }

        public List<RenderPair> SortedOrderPairs { get; private set; }

        private RenderGroupComparer comparer;
        private EntityQuery unsortedQuery;

        protected override void OnCreate() {
            comparer = new RenderGroupComparer();

            unsortedQuery = GetEntityQuery(new EntityQueryDesc {
                All = new [] {
                    ComponentType.ReadOnly<UnsortedRenderTag>(),
                    ComponentType.ReadOnly<RenderGroupID>(),
                    ComponentType.ReadOnly<RenderElement>()
                }
            });

            SortedOrderPairs = new List<RenderPair>();
        }

        protected override void OnUpdate() {
            Entities.With(unsortedQuery).ForEach((Entity entity, ref RenderGroupID c0) => {
                SortedOrderPairs.Add(new RenderPair {
                    Root = entity,
                    ID   = c0.Value
                });

                PostUpdateCommands.RemoveComponent<UnsortedRenderTag>(entity);
            });

            SortedOrderPairs.Sort(comparer);
        }
    }

    /// <summary>
    /// Constructs the batches required to do rendering.
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    [UpdateAfter(typeof(BuildMeshSystem))]
    public class RenderBatchSystem : ComponentSystem {

        private EntityArchetype renderBatchArchetype;
        private EntityQuery canvasQuery;
        private List<Entity> batchedEntityList;

        protected override void OnCreate() {
            canvasQuery = GetEntityQuery(new EntityQueryDesc {
                All = new[] {
                    ComponentType.ReadOnly<CanvasSortOrder>(),
                    ComponentType.ReadOnly<DirtyTag>(),
                    ComponentType.ReadOnly<Child>()
                }
            });

            renderBatchArchetype = EntityManager.CreateArchetype(new [] {
                ComponentType.ReadWrite<RenderGroupID>(),
                ComponentType.ReadWrite<RenderElement>(),
                ComponentType.ReadOnly<UnsortedRenderTag>()
            });

            batchedEntityList = new List<Entity>();

            RequireForUpdate(canvasQuery);
        }

        protected override void OnUpdate() {
            var childrenBuffer = GetBufferFromEntity<Child>(true);

            Entities.With(canvasQuery).ForEach((Entity entity, CanvasSortOrder s0, DynamicBuffer<Child> b0) => {
                // Clear the list so that we can build a render hierarchy.
                batchedEntityList.Clear();

                var renderBatchEntity = PostUpdateCommands.CreateEntity(renderBatchArchetype);
                PostUpdateCommands.SetComponent(renderBatchEntity, new RenderGroupID { Value = s0.Value });
                var buffer = PostUpdateCommands.AddBuffer<RenderElement>(renderBatchEntity);

                RecurseChildren(in b0, in childrenBuffer);

                buffer.ResizeUninitialized(batchedEntityList.Count);

                for (int i = 0; i < buffer.Length; i++) {
                    buffer[i] = new RenderElement { Value = batchedEntityList[i] };
                }

                PostUpdateCommands.RemoveComponent<DirtyTag>(entity);
            });
        }

        private void RecurseChildren(in DynamicBuffer<Child> children, in BufferFromEntity<Child> childBuffer) {
            for (int i = 0; i < children.Length; i++) {
                var child = children[i].Value;

                if (childBuffer.Exists(child)) {
                    var grandChildren = childBuffer[child];
                    RecurseChildren(in grandChildren, in childBuffer);
                }

                batchedEntityList.Add(child);
            }
        }
    }
}