using System;
using Sirenix.OdinInspector;

namespace WFramework.CoreGameDevKit.OldNumericSystem
{
    [Serializable]
    [Obsolete]
    public abstract class OldNumericModifier
    {
        [LabelText("Tag")]  public string[] tags  = Array.Empty<string>();
        [LabelText("名称")]   public string   name  = "";
        [LabelText("叠加数量")] public int      count = 1;

        public virtual void ApplyModifier(ref int input, int source, OldNumeric oldNumeric) { }

        public virtual void ApplyModifier(ref float input, float source, OldNumeric oldNumeric) { }

        public abstract bool WeakEquals(OldNumericModifier other);
    }
}