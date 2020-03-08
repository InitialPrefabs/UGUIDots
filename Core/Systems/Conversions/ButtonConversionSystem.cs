using Unity.Entities;
using UnityEngine.UI;

namespace UGUIDots.Conversions.Systems {

    [UpdateAfter(typeof(ImageConversionSystem))]
    public class ButtonConversionSystem : GameObjectConversionSystem {
        protected override void OnUpdate() {
            Entities.ForEach((Button button) => {
                var entity = GetPrimaryEntity(button);

                var colorBlock = button.colors;
                DstEntityManager.SetComponentData(entity, new AppliedColor { Value = colorBlock.normalColor });
                DstEntityManager.AddComponentData(entity, ClickState.Default());
                DstEntityManager.AddComponentData(entity, ButtonVisual.Default());
                DstEntityManager.AddComponentData(entity, ColorStates.FromColorBlock(colorBlock));
            });
        }
    }
}
