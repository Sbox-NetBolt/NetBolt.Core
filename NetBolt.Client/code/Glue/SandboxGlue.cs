using NetBolt.Shared;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Utility;
using Sandbox.Internal;
using System;
using System.Collections.Generic;

namespace NetBolt.Client;

/// <summary>
/// The glue for running NetBolt on Sandbox.
/// </summary>
internal class SandboxGlue : IGlue
{
	/// <inheritdoc/>
	public string RealmName => "sandbox-client";
	/// <inheritdoc/>
	public bool IsServer => false;
	/// <inheritdoc/>
	public bool IsClient => true;

	/// <inheritdoc/>
	public ILogger Logger => _loggerGlue;
	/// <summary>
	/// The logger glue.
	/// </summary>
	private readonly LoggerGlue _loggerGlue = new();

	/// <inheritdoc/>
	public ITypeLibrary TypeLibrary => _typeLibraryGlue;
	/// <summary>
	/// The type library glue.
	/// </summary>
	private readonly TypeLibraryGlue _typeLibraryGlue = new();

	/// <inheritdoc/>
	public INetBoltServer Server => throw new NotSupportedException();

	/// <inheritdoc/>
	public INetBoltClient Client => _clientGlue;
	/// <summary>
	/// The client specific glue.
	/// </summary>
	private readonly ClientSpecificGlue _clientGlue = new();

	/// <inheritdoc/>
	public IReadOnlyList<INetworkClient> Clients => NetworkManager.Instance.Clients;
	/// <inheritdoc/>
	public INetworkClient LocalClient => NetworkManager.Instance.LocalClient;

	/// <inheritdoc/>
	public INetworkClient GetBot( long botId )
	{
		return new NetworkClient( botId, true );
	}

	/// <inheritdoc/>
	public INetworkClient GetClient( long clientId )
	{
		return new NetworkClient( clientId, false );
	}

	/// <summary>
	/// The glue required for logging in NetBolt.
	/// </summary>
	private class LoggerGlue : ILogger
	{
		/// <inheritdoc/>
		public void Error( string message )
		{
			Log.Error( message );
		}

		/// <inheritdoc/>
		public void Error<T>( string messageTemplate, T obj0 )
		{
			Log.Error( messageTemplate.Replace( "{0}", obj0?.ToString() ?? "null" ) );
		}

		/// <inheritdoc/>
		public void Error<T1, T2>( string messageTemplate, T1 obj0, T2 obj1 )
		{
			Log.Error( messageTemplate
				.Replace( "{0}", obj0?.ToString() ?? "null" )
				.Replace( "{1}", obj1?.ToString() ?? "null" ) );
		}

		/// <inheritdoc/>
		public void Error<T1, T2, T3>( string messageTemplate, T1 obj0, T2 obj1, T3 obj2 )
		{
			Log.Error( messageTemplate
				.Replace( "{0}", obj0?.ToString() ?? "null" )
				.Replace( "{1}", obj1?.ToString() ?? "null" )
				.Replace( "{2}", obj2?.ToString() ?? "null" ) );
		}

		/// <inheritdoc/>
		public void Warning<T>( string messageTemplate, T obj0 )
		{
			Log.Warning( messageTemplate.Replace( "{0}", obj0?.ToString() ?? "null" ) );
		}
	}

	/// <summary>
	/// The glue required for reflection actions in NetBolt.
	/// </summary>
	private class TypeLibraryGlue : ITypeLibrary
	{
		/// <inheritdoc/>
		public T? Create<T>( Type type )
		{
			return GlobalGameNamespace.TypeLibrary.Create<T>( type );
		}

		/// <inheritdoc/>
		public T? Create<T>( Type type, Type[] genericTypes )
		{
			return GlobalGameNamespace.TypeLibrary.GetDescription( type ).CreateGeneric<T>( genericTypes );
		}

		/// <inheritdoc/>
		public IEnumerable<IProperty> GetAllProperties( Type type )
		{
			foreach ( var property in GlobalGameNamespace.TypeLibrary.GetDescription( type ).Properties )
				yield return new PropertyDescriptionWrapper( property );
		}

		/// <inheritdoc/>
		public Type[] GetGenericArguments( Type type )
		{
			return GlobalGameNamespace.TypeLibrary.GetDescription( type ).GenericArguments;
		}

		/// <inheritdoc/>
		public IMethod? GetMethodByName( Type type, string methodName )
		{
			foreach ( var method in GlobalGameNamespace.TypeLibrary.FindStaticMethods( methodName ) )
			{
				if ( method.TypeDescription.TargetType == type )
					return new MethodDescriptionWrapper( method );
			}

			return null;
		}

		/// <inheritdoc/>
		public Type? GetTypeByName( string typeName )
		{
			return GlobalGameNamespace.TypeLibrary.GetDescription( typeName ).TargetType;
		}

		/// <inheritdoc/>
		public bool IsClass( Type type )
		{
			return GlobalGameNamespace.TypeLibrary.GetDescription( type ).IsClass;
		}

		/// <inheritdoc/>
		public bool IsStruct( Type type )
		{
			var description = GlobalGameNamespace.TypeLibrary.GetDescription( type );
			return description.IsValueType && !description.IsEnum;
		}
	}


	/// <summary>
	/// The glue for client specific functionality.
	/// </summary>
	private class ClientSpecificGlue : INetBoltClient
	{
		/// <inheritdoc/>
		public void SendToServer( NetworkMessage message )
		{
			NetworkManager.Instance.SendToServer( message );
		}
	}
}
