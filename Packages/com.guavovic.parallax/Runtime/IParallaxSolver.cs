namespace Guavovic.Parallax
{
    public interface IParallaxSolver
    {
        void Solve(ParallaxLayer layer, ParallaxLayerSettings settings, in ParallaxContext context);
    }
}
