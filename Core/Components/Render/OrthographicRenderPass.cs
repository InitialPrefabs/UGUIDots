using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UGUIDots.Render {

    public class OrthographicRenderPass : ScriptableRenderPass {

        public CommandBuffer CommandBuffer;

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
            if (CommandBuffer != null) {
                context.ExecuteCommandBuffer(CommandBuffer);
            }
        }
    }
}
