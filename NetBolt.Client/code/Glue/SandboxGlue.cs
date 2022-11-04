using NetBolt.Shared;
using NetBolt.Shared.Clients;
using NetBolt.Shared.Messages;
using NetBolt.Shared.Networkables;
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
	public INetBoltServer Server => null!;

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
		public IEnumerable<Type> GetAllNetworkableTypes()
		{
			foreach ( var description in GlobalGameNamespace.TypeLibrary.GetDescriptions<INetworkable>() )
				yield return description.TargetType;
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
		public ushort GetIdentifierFromNetworkableType( Type type )
		{
			if ( !type.IsAssignableTo( typeof( INetworkable ) ) )
			{
				Log.Error( $"{type} does not implement {typeof( INetworkable )}" );
				return 0;
			}

			return NetworkManager.Instance.NetworkableTypeCache[type];
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
		public Type? GetNetworkableTypeByIdentifier( ushort identifier )
		{
			foreach ( var (type, id) in NetworkManager.Instance.NetworkableTypeCache )
			{
				if ( id == identifier )
					return type;
			}

			return null;
		}

		/// <inheritdoc/>
		public Type? GetTypeByName( string typeName )
		{
			// TODO: Support full name when fixed https://github.com/sboxgame/issues/issues/2413.
			if ( typeName.Contains( '.' ) )
				typeName = typeName.Split( '.' )[^1];

			Log.Info( typeName );
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
			// TODO: Remove this once https://github.com/sboxgame/issues/issues/2412 is fixed.
			if ( description is null )
				return false;

			return description.IsValueType && !description.IsEnum;
		}
	}


	/// <summary>
	/// The glue for client specific functionality.
	/// </summary>
	internal class ClientSpecificGlue : INetBoltClient
	{
		/// <summary>
		/// A dictionary containing all current <see cref="ComplexNetworkable"/> requests.
		/// </summary>
		private readonly Dictionary<int, List<Action<ComplexNetworkable>>> RequestCallbacks = new();
		/// <summary>
		/// A dictionary containing all current <see cref="Shared.Entities.IEntity"/> requests.
		/// </summary>
		private readonly Dictionary<int, List<Action<Shared.Entities.IEntity>>> EntityRequestCallbacks = new();

		/// <summary>
		/// Triggers any request callbacks that needs the <see cref="ComplexNetworkable"/> provided.
		/// </summary>
		/// <param name="complexNetworkable">The <see cref="ComplexNetworkable"/> that has become available to the client.</param>
		internal void ComplexNetworkableAvailable( ComplexNetworkable complexNetworkable )
		{
			var networkId = complexNetworkable.NetworkId;
			if ( EntityRequestCallbacks.ContainsKey( networkId ) && complexNetworkable is Shared.Entities.IEntity entity )
			{
				foreach ( var cb in EntityRequestCallbacks[networkId] )
					cb( entity );
				EntityRequestCallbacks.Remove( networkId );
			}
			else if ( RequestCallbacks.ContainsKey( networkId ) )
			{
				foreach ( var cb in RequestCallbacks[networkId] )
					cb( complexNetworkable );
				RequestCallbacks.Remove( networkId );
			}
		}

		/// <inheritdoc/>
		public void RequestComplexNetworkable( int networkId, Action<ComplexNetworkable> cb )
		{
			if ( !RequestCallbacks.ContainsKey( networkId ) )
				RequestCallbacks.Add( networkId, new List<Action<ComplexNetworkable>>() );

			RequestCallbacks[networkId].Add( cb );
		}

		/// <inheritdoc/>
		public void RequestEntity( int networkId, Action<Shared.Entities.IEntity> cb )
		{
			if ( !EntityRequestCallbacks.ContainsKey( networkId ) )
				EntityRequestCallbacks.Add( networkId, new List<Action<Shared.Entities.IEntity>>() );

			EntityRequestCallbacks[networkId].Add( cb );
		}

		/// <inheritdoc/>
		public void SendToServer( NetworkMessage message )
		{
			NetworkManager.Instance.SendToServer( message );
		}
	}
}
