public record CountryCodeData
(
	int Index, 
	string Code, 
	string Name
) : TableRowData<int>(Index);
