using System;

namespace NetBolt.Shared.Utility;

/// <summary>
/// Marks a target to signify that it should only be utilized in the client realm.
/// </summary>
[AttributeUsage( AttributeTargets.All, AllowMultiple = false, Inherited = true )]
public class ClientOnlyAttribute : Attribute
{
}
