namespace Dragonfly.SiteAuditor.Models;

using System;

public class DatesOption
{
	public DateTime StartDate { get; set; }
	public DateTime EndDate { get; set; }
	public string Description { get; set; }= "";

	public DatesOption(DateTime Start, DateTime End, string Description)
	{
		this.StartDate = Start;
		this.EndDate = End;
		this.Description = Description;
	}

	public DatesOption()
	{
		
	}
}

