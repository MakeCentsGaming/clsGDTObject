# clsGDTObject
This is a gdt parser that creates and object out of a list of gdts, which includes objects of each asset/gdf


GDT gdt = new GDT();

//optional actions

gdt.ProgressBar += progressbar;

gdt.MessageBar += messagebox;

//Either do one gdt:

gdt.SplitGDT(string);//full path to gdt

//Or pass a list of gdts

gdt.ReadEachGDT(List<string>);//list of full gdt paths

See xml for full list of functions
