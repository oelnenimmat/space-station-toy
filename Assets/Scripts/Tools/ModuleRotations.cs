using UnityEngine;

public enum ModuleRotations
{
    None,

    XAxisToRightAngles,
    YAxisToRightAngles,
    ZAxisToRightAngles,
    
    XAxisToRightAnglesAndOpposites,
    YAxisToRightAnglesAndOpposites,
    ZAxisToRightAnglesAndOpposites,
    
    All
}

public static class ModuleRotationsExtensions
{
    public static Quaternion[] GetRotations(this ModuleRotations r)
    {
        switch(r)
        {
            case ModuleRotations.None:
            {
                return new Quaternion[] {Quaternion.identity};
            }

            case ModuleRotations.XAxisToRightAngles:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(0, 90, 0),
                    Quaternion.Euler(0, 0, 90),
                };
            }

            case ModuleRotations.YAxisToRightAngles:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(90, 0, 0),
                    Quaternion.Euler(0, 0, 90),
                };
            }


            case ModuleRotations.ZAxisToRightAngles:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(0, 90, 0),
                    Quaternion.Euler(90, 0, 0),
                };
            }

            case ModuleRotations.XAxisToRightAnglesAndOpposites:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(0, 90, 0),
                    Quaternion.Euler(0, 180, 0),
                    Quaternion.Euler(0, 270, 0),
                    Quaternion.Euler(0, 0, 90),
                    Quaternion.Euler(0, 0, 270),
                };
            }

            case ModuleRotations.YAxisToRightAnglesAndOpposites:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(90, 0, 0),
                    Quaternion.Euler(180, 0, 0),
                    Quaternion.Euler(270, 0, 0),
                    Quaternion.Euler(0, 0, 90),
                    Quaternion.Euler(0, 0, 270),
                };
            }


            case ModuleRotations.ZAxisToRightAnglesAndOpposites:
            {
                return new Quaternion[]
                {
                    Quaternion.identity,
                    Quaternion.Euler(0, 90, 0),
                    Quaternion.Euler(0, 180, 0),
                    Quaternion.Euler(0, 270, 0),
                    Quaternion.Euler(90, 0, 0),
                    Quaternion.Euler(270, 0, 0),
                };
            }

            case ModuleRotations.All:
            {
                return new Quaternion[24]
                {
                    // -X up
                    Quaternion.Euler(0, 0, 270),
                    Quaternion.Euler(0, 90, 270),
                    Quaternion.Euler(0, 180, 270),
                    Quaternion.Euler(0, 270, 270),

                    // +X up
                    Quaternion.Euler(0, 0, 90),
                    Quaternion.Euler(0, 90, 90),
                    Quaternion.Euler(0, 180, 90),
                    Quaternion.Euler(0, 270, 90),

                    // -Y up
                    Quaternion.Euler(0, 0, 180),
                    Quaternion.Euler(0, 90, 180),
                    Quaternion.Euler(0, 180, 180),
                    Quaternion.Euler(0, 270, 180),

                    // +Y up
                    Quaternion.Euler(0, 0, 0),
                    Quaternion.Euler(0, 90, 0),
                    Quaternion.Euler(0, 180, 0),
                    Quaternion.Euler(0, 270, 0),

                    // -Z up
                    Quaternion.Euler(90, 0, 0),
                    Quaternion.Euler(90, 90, 0),
                    Quaternion.Euler(90, 180, 0),
                    Quaternion.Euler(90, 270, 0),

                    // +Z up
                    Quaternion.Euler(270, 0, 0),
                    Quaternion.Euler(270, 90, 0),
                    Quaternion.Euler(270, 180, 0),
                    Quaternion.Euler(270, 270, 0),
                };
            }

            default:
                return GetRotations(ModuleRotations.All);
        }
    }
}