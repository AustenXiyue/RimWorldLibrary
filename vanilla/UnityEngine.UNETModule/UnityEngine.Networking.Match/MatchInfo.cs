using System;
using UnityEngine.Networking.Types;

namespace UnityEngine.Networking.Match;

[Obsolete("The matchmaker and relay feature will be removed in the future, minimal support will continue until this can be safely done.")]
public class MatchInfo
{
	public string address { get; private set; }

	public int port { get; private set; }

	public int domain { get; private set; }

	public NetworkID networkId { get; private set; }

	public NetworkAccessToken accessToken { get; private set; }

	public NodeID nodeId { get; private set; }

	public bool usingRelay { get; private set; }

	public MatchInfo()
	{
	}

	internal MatchInfo(CreateMatchResponse matchResponse)
	{
		address = matchResponse.address;
		port = matchResponse.port;
		domain = matchResponse.domain;
		networkId = (NetworkID)matchResponse.networkId;
		accessToken = new NetworkAccessToken(matchResponse.accessTokenString);
		nodeId = matchResponse.nodeId;
		usingRelay = matchResponse.usingRelay;
	}

	internal MatchInfo(JoinMatchResponse matchResponse)
	{
		address = matchResponse.address;
		port = matchResponse.port;
		domain = matchResponse.domain;
		networkId = (NetworkID)matchResponse.networkId;
		accessToken = new NetworkAccessToken(matchResponse.accessTokenString);
		nodeId = matchResponse.nodeId;
		usingRelay = matchResponse.usingRelay;
	}

	public override string ToString()
	{
		return UnityString.Format("{0} @ {1}:{2} [{3},{4}]", networkId, address, port, nodeId, usingRelay);
	}
}
