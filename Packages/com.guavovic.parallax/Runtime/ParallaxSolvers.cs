namespace Guavovic.Parallax
{
    public static class ParallaxSolvers
    {
        public static IParallaxSolver Create(ParallaxMode mode)
        {
            return mode == ParallaxMode.Perspective
                ? (IParallaxSolver)new PerspectiveSolver()
                : new Simulated2DSolver();
        }
    }
}
