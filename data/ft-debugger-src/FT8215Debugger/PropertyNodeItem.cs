using System.Collections.Generic;

namespace FT8215Debugger;

public class PropertyNodeItem
{
	public string Icon { get; set; }

	public string EditIcon { get; set; }

	public string DisplayName { get; set; }

	public string Name { get; set; }

	public bool IsGroup { get; set; }

	public int ArticleId { get; set; }

	public List<PropertyNodeItem> Children { get; set; }

	public PropertyNodeItem()
	{
		Children = new List<PropertyNodeItem>();
	}
}
