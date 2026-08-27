using Ehsms.BuildingBlocks;
using Xunit;
namespace Ehsms.UnitTests;
public sealed class ArchitectureMarkerTests { [Fact] public void Marker_identifies_building_blocks_assembly() => Assert.Equal("Ehsms.BuildingBlocks", typeof(ArchitectureMarker).Assembly.GetName().Name); }
