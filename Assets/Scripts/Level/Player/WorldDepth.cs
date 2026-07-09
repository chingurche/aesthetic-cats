    public static class WorldDepth
    {
        public const float StartDepth = 50f;
        public const float DepthPerY = 2.5f;

        public static float YToDepth(float y)
        {
            return StartDepth - y * DepthPerY;
        }

        public static float DepthToY(float depth)
        {
            return (StartDepth - depth) / DepthPerY;
        }
    }