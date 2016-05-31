using CakewalkIoC.Core;

public class FortressBootStrapper : BaseBootStrapper {

    public override void Configure(Container container)
    {
        container.Register<TileManager>();
    }
    
}
