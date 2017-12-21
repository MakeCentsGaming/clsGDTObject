# clsGDTObject
This is a gdt parser that creates and object out of a gdt, or list of gdts, which includes objects of each asset/gdf. Static utilities included such as kvp and name/asset formatting with addition of prefix and suffix ability.


### GDT object
- GDT gdt = new GDT();

##### optional actions
- gdt.ProgressBar += progressbar;
- gdt.MessageBar += messagebox;

##### Either do one gdt:
- gdt.SplitGDT(string); //full path to gdt

##### Or pass a list of gdts
- gdt.ReadEachGDT(List<string>); //list of full gdt paths

##### See xml for full list of functions


This is a work in progress and will be updated as I find more things I need to do to gdts.
