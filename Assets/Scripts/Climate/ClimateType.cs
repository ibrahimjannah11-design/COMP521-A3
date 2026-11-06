namespace Sim
{
    public enum ClimateType { None = 0, Fire, Flood, Desert, Plant }

    public static class ClimateTypeExt
    {
        //thuis just habdles speed mutls for each ckimate
        public static float SpeedMultiplier(this ClimateType t, bool isBot, SimConfig c)
        {
            switch (t)
            {
                case ClimateType.Fire:   return c.fireSpeed;
                case ClimateType.Flood:  return c.floodSpeed;
                case ClimateType.Desert: return c.desertSpeed;
                case ClimateType.Plant:  return isBot ? 1f : c.plantSpeed;
                default:                 return 1f;
            }
        }
    }
}
