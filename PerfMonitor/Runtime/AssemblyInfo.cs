using UnityEngine.Scripting;

// Nothing in a game references this package directly, so without this IL2CPP code stripping
// removes it completely and the overlay and PerfScope bridge never start.
[assembly: AlwaysLinkAssembly]
