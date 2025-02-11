using System.IO;
using MS.Internal.Globalization;

namespace System.Windows.Markup.Localizer;

/// <summary>Extracts resources from a BAML file and generates a localized version of a BAML source.</summary>
public class BamlLocalizer
{
	private BamlTreeMap _bamlTreeMap;

	private BamlTree _tree;

	/// <summary>Occurs when the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizer" /> encounters abnormal conditions.</summary>
	public event BamlLocalizerErrorNotifyEventHandler ErrorNotify;

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizer" /> class with the specified BAML source stream. </summary>
	/// <param name="source">A file stream that contains the BAML input to be localized.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public BamlLocalizer(Stream source)
		: this(source, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizer" /> class with the specified localizability resolver and BAML source stream. </summary>
	/// <param name="source">A file stream that contains the BAML input to be localized.</param>
	/// <param name="resolver">An instance of <see cref="T:System.Windows.Markup.Localizer.BamlLocalizabilityResolver" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public BamlLocalizer(Stream source, BamlLocalizabilityResolver resolver)
		: this(source, resolver, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Windows.Markup.Localizer.BamlLocalizer" /> class with the specified localizability resolver, BAML source stream, and <see cref="T:System.IO.TextReader" />.</summary>
	/// <param name="source">A file stream that contains the BAML input to be localized.</param>
	/// <param name="resolver">An instance of <see cref="T:System.Windows.Markup.Localizer.BamlLocalizabilityResolver" />.</param>
	/// <param name="comments">Reads the localized XML comments associated with this BAML input.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="source" /> is null.</exception>
	public BamlLocalizer(Stream source, BamlLocalizabilityResolver resolver, TextReader comments)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		_tree = BamlResourceDeserializer.LoadBaml(source);
		_bamlTreeMap = new BamlTreeMap(this, _tree, resolver, comments);
	}

	/// <summary>Extracts all localizable resources from a BAML stream. </summary>
	/// <returns>A copy of the localizable resources from a BAML stream, in the form of a <see cref="T:System.Windows.Markup.Localizer.BamlLocalizationDictionary" />.</returns>
	public BamlLocalizationDictionary ExtractResources()
	{
		return _bamlTreeMap.LocalizationDictionary.Copy();
	}

	/// <summary>Applies resource updates to the BAML source and writes the updated version to a specified stream in order to create a localized version of the source BAML. </summary>
	/// <param name="target">The stream that will receive the updated BAML.</param>
	/// <param name="updates">The resource updates to be applied to the source BAML.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="target" /> or <paramref name="updates" /> are null.</exception>
	public void UpdateBaml(Stream target, BamlLocalizationDictionary updates)
	{
		if (target == null)
		{
			throw new ArgumentNullException("target");
		}
		if (updates == null)
		{
			throw new ArgumentNullException("updates");
		}
		BamlTree tree = _tree.Copy();
		_bamlTreeMap.EnsureMap();
		BamlTreeUpdater.UpdateTree(tree, _bamlTreeMap, updates);
		BamlResourceSerializer.Serialize(this, tree, target);
	}

	/// <summary>Raises the <see cref="E:System.Windows.Markup.Localizer.BamlLocalizer.ErrorNotify" /> event.</summary>
	/// <param name="e">Required event arguments.</param>
	protected virtual void OnErrorNotify(BamlLocalizerErrorNotifyEventArgs e)
	{
		this.ErrorNotify?.Invoke(this, e);
	}

	internal void RaiseErrorNotifyEvent(BamlLocalizerErrorNotifyEventArgs e)
	{
		OnErrorNotify(e);
	}
}
