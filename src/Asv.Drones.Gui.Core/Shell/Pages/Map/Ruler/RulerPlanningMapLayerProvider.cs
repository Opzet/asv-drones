using System.ComponentModel.Composition;
using DynamicData;

namespace Asv.Drones.Gui.Core;

[Export(PlaningPageViewModel.UriString, typeof(IViewModelProvider<IMapAnchor>))]
[PartCreationPolicy(CreationPolicy.NonShared)]
public class RulerPlanningMapLayerProvider : ViewModelProviderBase<IMapAnchor>
{
    [ImportingConstructor]
    public RulerPlanningMapLayerProvider(ILocalizationService loc)
    {
        var ruler = new Ruler();
        
        Source.AddOrUpdate(new RulerAnchor("1", ruler, RulerPosition.Start, loc));
        Source.AddOrUpdate(new RulerAnchor("2", ruler, RulerPosition.Stop, loc));
        Source.AddOrUpdate(new RulerPolygon(ruler));
    }
}