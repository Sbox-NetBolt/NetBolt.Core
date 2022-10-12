using System;

namespace NetBolt.Shared.Networkables;

/// <summary>
/// Marks a potentially networkable property to not be networked.
/// </summary>
[AttributeUsage( AttributeTargets.Property )]
public class NoNetworkAttribute : Attribute
{
}
