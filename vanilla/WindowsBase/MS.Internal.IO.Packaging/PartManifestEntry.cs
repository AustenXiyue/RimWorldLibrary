using System;
using System.Collections.Generic;
using System.IO.Packaging;

namespace MS.Internal.IO.Packaging;

internal struct PartManifestEntry
{
	private Uri _owningPartUri;

	private Uri _uri;

	private ContentType _contentType;

	private string _hashAlgorithm;

	private string _hashValue;

	private List<string> _transforms;

	private List<PackageRelationshipSelector> _relationshipSelectors;

	internal bool IsRelationshipEntry => _relationshipSelectors != null;

	internal Uri Uri => _uri;

	internal ContentType ContentType => _contentType;

	internal string HashAlgorithm => _hashAlgorithm;

	internal string HashValue => _hashValue;

	internal List<string> Transforms => _transforms;

	internal List<PackageRelationshipSelector> RelationshipSelectors => _relationshipSelectors;

	internal Uri OwningPartUri => _owningPartUri;

	internal PartManifestEntry(Uri uri, ContentType contentType, string hashAlgorithm, string hashValue, List<string> transforms, List<PackageRelationshipSelector> relationshipSelectors)
	{
		Invariant.Assert(uri != null);
		Invariant.Assert(contentType != null);
		Invariant.Assert(hashAlgorithm != null);
		_uri = uri;
		_contentType = contentType;
		_hashAlgorithm = hashAlgorithm;
		_hashValue = hashValue;
		_transforms = transforms;
		_relationshipSelectors = relationshipSelectors;
		_owningPartUri = null;
		if (_relationshipSelectors != null)
		{
			Invariant.Assert(relationshipSelectors.Count > 0);
			_owningPartUri = relationshipSelectors[0].SourceUri;
		}
	}
}
