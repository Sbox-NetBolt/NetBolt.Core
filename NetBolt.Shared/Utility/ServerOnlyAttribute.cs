using System;

namespace NetBolt.Shared.Utility;

/// <summary>
/// Marks a target to signify that it should only be utilized in the server realm.
/// </summary>
[AttributeUsage( AttributeTargets.All, AllowMultiple = false, Inherited = true )]
public class ServerOnlyAttribute : Attribute
{
}
