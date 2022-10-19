using System;

namespace NetBolt.Shared.Networkables;

/// <summary>
/// Marks a networkable property to be lerped on the client when a change occurs.
/// </summary>
[AttributeUsage( AttributeTargets.Property )]
public class LerpAttribute : Attribute
{
}
