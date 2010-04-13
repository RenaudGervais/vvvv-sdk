using System;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.HDE.Interfaces;

/// <summary>
/// Version 1 of the VVVV PluginInterface.
/// DirectX/SlimDX related parts are in a separate file: PluginDXInterface1.cs
///
/// To convert this to a typelib make sure AssemblyInfo.cs states: ComVisible(true).
/// Then on a commandline type:
/// <c>regasm _PluginInterfaces.dll /tlb</c>
/// This generates and registers the typelib which can then be imported e.g. via Delphi:Components:Import Component:Import Typelib
/// </summary>
namespace VVVV.PluginInterfaces.V1
{
	#region enums
	/// <summary>
	/// Used in the pin creating functions of <see cref="VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy possible SliceCounts.
	/// </summary>
	public enum TSliceMode {
		/// <summary>
		/// The pin can only have one slice.
		/// </summary>
		Single,
		/// <summary>
		/// The pin can have any number of slices.
		/// </summary>
		Dynamic};
	
	/// <summary>
	/// Used to set the <see cref="VVVV.PluginInterfaces.V1.PluginInfo.InitialComponentMode">InitialComponentMode</see>
	/// in <see cref="VVVV.PluginInterfaces.V1.PluginInfo">IPluginInfo</see> which specifies the ComponentMode
	/// for a plugin when it is being created.
	/// </summary>
	public enum TComponentMode {
		/// <summary>
		/// The plugins GUI will initially be hidden, only its node is visible.
		/// </summary>
		Hidden,
		/// <summary>
		/// The plugins GUI will initially be showing in a box in the patch.
		/// </summary>
		InABox,
		/// <summary>
		/// The plugins GUI will initially be showing in its own window.
		/// </summary>
		InAWindow};
	
	/// <summary>
	/// Used in the pin creating functions of <see cref="VVVV.PluginInterfaces.V1.IPluginHost">IPluginHost</see> to specifiy the initial visibility of the pin.
	/// If this is not set to FALSE then the option can be changed by the user via the Inspektor.
	/// </summary>
	public enum TPinVisibility {
		/// <summary>
		/// The pin is not visible at all.
		/// </summary>
		False,
		/// <summary>
		/// The pin is visible only in the Inspektor
		/// </summary>
		OnlyInspector,
		/// <summary>
		/// The pin is not visible on the node, but space is reserved for it and it appears on mouseover.
		/// </summary>
		Hidden,
		/// <summary>
		/// Default. The pin is visible on the node.
		/// </summary>
		True};
	
	/// <summary>
	/// Used to specifiy a pins Direction.
	/// </summary>
	public enum TPinDirection {
		/// <summary>
		/// The pin is a ConfigurationPin and as such only accessible via the Inspektor.
		/// </summary>
		Configuration,
		/// <summary>
		/// The pin is an input to the node.
		/// </summary>
		Input,
		/// <summary>
		/// The pin is an output from the node.
		/// </summary>
		Output};
	
	/// <summary>
	/// Used in the <see cref="VVVV.PluginInterfaces.V1.IPluginHost.Log()">IPluginHost.Log</see> function to specify the type of the log message.
	/// </summary>
	public enum TLogType {
		/// <summary>
		/// Specifies a debug message.
		/// </summary>
		Debug,
		/// <summary>
		/// Specifies an ordinary message.
		/// </summary>
		Message,
		/// <summary>
		/// Specifies a warning message.
		/// </summary>
		Warning,
		/// <summary>
		/// Specifies an errormessage.
		/// </summary>
		/// 
		Error};
	
	/// <summary>
	/// Used in <see cref="VVVV.PluginInterfaces.V1.INodeInfo">INodeInfo</see> to specify the type of the provided node.
	/// </summary>
	public enum TNodeType {
		/// <summary>
		/// Specifies a native node.
		/// </summary>
		Native,
		/// <summary>
		/// Specifies a freeframe node.
		/// </summary>
		Freeframe,
		/// <summary>
		/// Specifies an effect node.
		/// </summary>
		Effect,
		/// <summary>
		/// Specifies a directshow node.
		/// </summary>
		Directshow,
		/// <summary>
		/// Specifies a static plugin node.
		/// </summary>
		Plugin,
		/// <summary>
		/// Specifies a dynamic plugin node.
		/// </summary>
		Dynamic};
	#endregion enums
	
	#region basic pins
	/// <summary>
	/// Base interface of all pin interfaces. Never used directly.
	/// </summary>
	[Guid("D3C5CB5C-C054-4AB6-AC04-6BDB34692B25"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginIO
	{
		/// <summary>
		/// The pins name.
		/// </summary>
		string Name{get; set;}
		/// <summary>
		/// The order property helps the node to arrange its pins visually. The higher the order, the more right the pin appears on the node.
		/// </summary>
		int Order{get; set;}
		/// <summary>
		/// Specifies whether the pin is connected in the patch or not.
		/// </summary>
		bool IsConnected{get;}
	}
	
	/// <summary>
	/// Base interface of all ConfigurationPin interfaces. Never used directly.
	/// </summary>
	[Guid("11FDCEBD-FFC0-415D-90D5-DA4DBBDB5B67"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginConfig: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get; set;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{get;}
	}
	
	/// <summary>
	/// Base interface of all InputPin interfaces. Never used directly.
	/// </summary>
	[Guid("68C6F37B-1D45-4683-9FC2-BC2580187D44"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginIn: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{get;}
		/// <summary>
		/// Returns whether any slice of this pin has been changed in the current frame. This information is typically used to determine if
		/// further processing is needed or can be ommited.
		/// </summary>
		bool PinIsChanged{get;}
	}
	
	/// <summary>
	/// Base interface of all fast InputPin interfaces. Never used directly.
	/// </summary>
	[Guid("9AFAD289-7C11-4296-B232-8B33FAC3E27D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginFastIn: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{get;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{get;}
	}
	
	/// <summary>
	/// Base interface of all OutputPin interfaces. Never used directly.
	/// </summary>
	[Guid("67FB9F25-0579-495C-8535-28CC15F54C55"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginOut: IPluginIO
	{
		/// <summary>
		/// The pins SliceCount specifies the number of Values (2D Vector, String...) it carries. This is like the length of an array or list.
		/// </summary>
		int SliceCount{set;}
		/// <summary>
		/// Returns a String of the pins concatenated Values. Typcally used internally only to save a pins state to disk.
		/// </summary>
		string SpreadAsString{set;}
	}
	
	#endregion basic pins
	
	#region value pins

	/// <summary>
	/// Interface to a ConfigurationPin of type Value.
	/// </summary>
	[Guid("46154821-A76F-4258-846D-8524957F98D4"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueConfig: IPluginConfig
	{
		/// <summary>
		/// Used to write a Value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Value to.</param>
		/// <param name="Value">The Value to write.</param>
		void SetValue(int Index, double Value);
		/// <summary>
		/// Used to write a 2D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 2D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		void SetValue2D(int Index, double Value1, double Value2);
		/// <summary>
		/// Used to write a 3D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 3D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		/// <param name="Value3">The Value to write to the 3rd dimension.</param>
		void SetValue3D(int Index, double Value1, double Value2, double Value3);
		/// <summary>
		/// Used to write a 4D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 4D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		/// <param name="Value3">The Value to write to the 3rd dimension.</param>
		/// <param name="Value4">The Value to write to the 4th dimension.</param>
		void SetValue4D(int Index, double Value1, double Value2, double Value3, double Value4);
		/// <summary>
		/// Used to write a Matrix to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Matrix to.</param>
		/// <param name="Value">The Matrix to write.</param>
		void SetMatrix(int Index, Matrix4x4 Value);
		
		/// <summary>
		/// Used to retrieve a Value from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Value from.</param>
		/// <param name="Value">The retrieved Value.</param>
		void GetValue(int Index, out double Value);
		/// <summary>
		/// Used to retrieve a 2D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 2D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		void GetValue2D(int Index, out double Value1, out double Value2);
		/// <summary>
		/// Used to retrieve a 3D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 3D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		/// <summary>
		/// Used to retrieve a 4D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		/// <param name="Value4">The retrieved 4th dimension of the Vector.</param>
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		/// <summary>
		/// Used to retrieve a 4x4 Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4x4 Matrix from.</param>
		/// <param name="Value">The retrieved 4x4 Matrix.</param>
		void GetMatrix(int Index, out Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin.
		/// </summary>
		/// <param name="SliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="Value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out int SliceCount, out double* Value);

		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Value.
	/// </summary>
	[Guid("40137258-9CDE-49F4-93BA-DE7D91007809"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueIn: IPluginIn				//value input pin
	{
		/// <summary>
		/// Used to retrieve a Value from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Value from.</param>
		/// <param name="Value">The retrieved Value.</param>
		void GetValue(int Index, out double Value);
		/// <summary>
		/// Used to retrieve a 2D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 2D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		void GetValue2D(int Index, out double Value1, out double Value2);
		/// <summary>
		/// Used to retrieve a 3D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 3D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		/// <summary>
		/// Used to retrieve a 4D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		/// <param name="Value4">The retrieved 4th dimension of the Vector.</param>
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		/// <summary>
		/// Used to retrieve a 4x4 Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4x4 Matrix from.</param>
		/// <param name="Value">The retrieved 4x4 Matrix.</param>
		void GetMatrix(int Index, out Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to retrive large Spreads of Values more efficiently.
		/// Attention: Don't use this Pointer to write Values to the pin!
		/// </summary>
		/// <param name="SliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="Value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out int SliceCount, out double* Value);
		
		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}

	/// <summary>
	/// Interface to a fast InputPin of type Value.
	/// </summary>
	[Guid("095081B7-D929-4459-83C0-18AA809E6635"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueFastIn: IPluginFastIn		//fast value input pin
	{
		/// <summary>
		/// Used to retrieve a Value from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Value from.</param>
		/// <param name="Value">The retrieved Value.</param>
		void GetValue(int Index, out double Value);
		/// <summary>
		/// Used to retrieve a 2D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 2D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		void GetValue2D(int Index, out double Value1, out double Value2);
		/// <summary>
		/// Used to retrieve a 3D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 3D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		void GetValue3D(int Index, out double Value1, out double Value2, out double Value3);
		/// <summary>
		/// Used to retrieve a 4D Vector from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4D Vector from.</param>
		/// <param name="Value1">The retrieved 1st dimension of the Vector.</param>
		/// <param name="Value2">The retrieved 2nd dimension of the Vector.</param>
		/// <param name="Value3">The retrieved 3rd dimension of the Vector.</param>
		/// <param name="Value4">The retrieved 4th dimension of the Vector.</param>
		void GetValue4D(int Index, out double Value1, out double Value2, out double Value3, out double Value4);
		/// <summary>
		/// Used to retrieve a 4x4 Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the 4x4 Matrix from.</param>
		/// <param name="Value">The retrieved 4x4 Matrix.</param>
		void GetMatrix(int Index, out Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to retrive large Spreads of Values more efficiently.
		/// Attention: Don't use this Pointer to write Values to the pin!
		/// </summary>
		/// <param name="SliceCount">The pins current SliceCount, specifying the number of values accessible via the Pointer.</param>
		/// <param name="Value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out int SliceCount, out double* Value);
		
		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Value.
	/// </summary>
	[Guid("B55B70E8-9C3D-408D-B9F9-A90CF8288FC7"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	unsafe public interface IValueOut: IPluginOut			//value output pin
	{
		/// <summary>
		/// Used to write a Value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Value to.</param>
		/// <param name="Value">The Value to write.</param>
		void SetValue(int Index, double Value);
		/// <summary>
		/// Used to write a 2D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 2D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		void SetValue2D(int Index, double Value1, double Value2);
		/// <summary>
		/// Used to write a 3D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 3D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		/// <param name="Value3">The Value to write to the 3rd dimension.</param>
		void SetValue3D(int Index, double Value1, double Value2, double Value3);
		/// <summary>
		/// Used to write a 4D Vector to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the 4D Vector to.</param>
		/// <param name="Value1">The Value to write to the 1st dimension.</param>
		/// <param name="Value2">The Value to write to the 2nd dimension.</param>
		/// <param name="Value3">The Value to write to the 3rd dimension.</param>
		/// <param name="Value4">The Value to write to the 4th dimension.</param>
		void SetValue4D(int Index, double Value1, double Value2, double Value3, double Value4);
		/// <summary>
		/// Used to write a Matrix to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Matrix to.</param>
		/// <param name="Value">The Matrix to write.</param>
		void SetMatrix(int Index, Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a Pointer to the Values of the pin, which can be used to write large number of values more efficiently.
		/// Note thought, that when writing Values to the Pointer the pins dimensions and overall SliceCount have to be taken care of manually.
		/// </summary>
		/// <param name="Value">A Pointer to the pins first Value.</param>
		void GetValuePointer(out double* Value);

		/// <summary>
		/// Used to set the SubType of a Value pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default">The Value the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType  (double Min, double Max, double StepSize, double Default, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 2D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType2D(double Min, double Max, double StepSize, double Default1, double Default2, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 3D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType3D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, bool IsBang, bool IsToggle, bool IsInteger);
		/// <summary>
		/// Used to set the SubType of a 4D Vector pin, which is a set of limitations to the pins value range, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" values on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Min">Minimum of the Values range.</param>
		/// <param name="Max">Maximum of the Values range.</param>
		/// <param name="StepSize">StepSize used when scrolling the value up or down via the GUI.</param>
		/// <param name="Default1">The Value the pins 1st dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default2">The Value the pins 2nd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default3">The Value the pins 3rd dimension is initialized with and can be reset to at any time.</param>
		/// <param name="Default4">The Value the pins 4th dimension is initialized with and can be reset to at any time.</param>
		/// <param name="IsBang">Hint to the GUI that this Value is a bang.</param>
		/// <param name="IsToggle">Hint to the GUI that this is a toggling Value.</param>
		/// <param name="IsInteger">Hint to the GUI that this is an integer Value.</param>
		void SetSubType4D(double Min, double Max, double StepSize, double Default1, double Default2, double Default3, double Default4, bool IsBang, bool IsToggle, bool IsInteger);
	}
	
	#endregion value pins
	
	#region string pins
	
	/// <summary>
	/// Interface to a ConfigurationPin of type String.
	/// </summary>
	[Guid("1FF25AD1-FBAB-4B29-8BAC-82CE53135868"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringConfig: IPluginConfig
	{
		/// <summary>
		/// Used to write a String to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the String to.</param>
		/// <param name="Value">The String to write.</param>
		void SetString(int Index, string Value);
		/// <summary>
		/// Used to retrieve a String from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the String from.</param>
		/// <param name="Value">The retrieved String.</param>
		void GetString(int Index, out string Value);
		/// <summary>
		/// Used to set the SubType of a String pin, which is a more detailed specification of the String, used by the GUI to guide the user to insert correct Strings.
		/// Note though that this does not prevent a user from setting "wrong" Strings on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The String the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsFilename">Hint to the GUI that this String is a filename</param>
		void SetSubType(string Default, bool IsFilename);
	}
	
	/// <summary>
	/// Interface to an InputPin of type String.
	/// </summary>
	[Guid("E329D418-20DE-4D91-B060-60EF2D73A7A6"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve a String from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the String from.</param>
		/// <param name="Value">The retrieved String.</param>
		void GetString(int Index, out string Value);
		/// <summary>
		/// Used to set the SubType of a String pin, which is a more detailed specification of the String, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" Strings on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The String the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsFilename">Hint to the GUI that this String is a filename</param>
		void SetSubType(string Default, bool IsFilename);
	}

	/// <summary>
	/// Interface to an OutputPin of type String.
	/// </summary>
	[Guid("EC32C616-A85F-42AC-B7D1-630E1F739D1D"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IStringOut: IPluginOut
	{
		/// <summary>
		/// Used to write a String to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the String to.</param>
		/// <param name="Value">The String to write.</param>
		void SetString(int Index, string Value);
		/// <summary>
		/// Used to set the SubType of a String pin, which is a more detailed specification of the String, used by the GUI to guide the user to insert correct values.
		/// Note though that this does not prevent a user from setting "wrong" Strings on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The String the pin is initialized with and can be reset to at any time.</param>
		/// <param name="IsFilename">Hint to the GUI that this String is a filename</param>
		void SetSubType(string Default, bool IsFilename);
	}
	
	#endregion string pins
	
	#region color pins
	
	/// <summary>
	/// Interface to a ConfigurationPin of type Color.
	/// </summary>
	[Guid("BAA49637-29FA-426A-9188-86906E660D30"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColorConfig: IPluginConfig
	{
		/// <summary>
		/// Used to write a Color to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Color to.</param>
		/// <param name="Color">The Color to write.</param>
		void SetColor(int Index, RGBAColor Color);
		/// <summary>
		/// Used to retrieve a Color from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Color from.</param>
		/// <param name="Color">The retrieved Color.</param>
		void GetColor(int Index, out RGBAColor Color);
		/// <summary>
		/// Used to set the SubType of a Color pin, which is a more detailed specification of the Color, used by the GUI to guide the user to insert correct Colors.
		/// Note though that this does not prevent a user from setting "wrong" Colors on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The Color the pin is initialized with and can be reset to at any time.</param>
		/// <param name="HasAlpha">Hint to the GUI that this Color has an alpha channel.</param>
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Color.
	/// </summary>
	[Guid("CB6289A8-28BD-4A52-9B7A-BC1092EA2FA5"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColorIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve a Color from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Color from.</param>
		/// <param name="Color">The retrieved Color.</param>
		void GetColor(int Index, out RGBAColor Color);
		/// <summary>
		/// Used to set the SubType of a Color pin, which is a more detailed specification of the Color, used by the GUI to guide the user to insert correct Colors.
		/// Note though that this does not prevent a user from setting "wrong" Colors on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The Color the pin is initialized with and can be reset to at any time.</param>
		/// <param name="HasAlpha">Hint to the GUI that this Color has an alpha channel.</param>
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Color.
	/// </summary>
	[Guid("432CE6BA-6F57-4387-A223-D2DAFA8125F0"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IColorOut: IPluginOut
	{
		/// <summary>
		/// Used to write a Color to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Color to.</param>
		/// <param name="Color">The Color to write.</param>
		void SetColor(int Index, RGBAColor Color);
		/// <summary>
		/// Used to set the SubType of a Color pin, which is a more detailed specification of the Color, used by the GUI to guide the user to insert correct Colors.
		/// Note though that this does not prevent a user from setting "wrong" Colors on a pin. Ultimately each node is responsible for dealing with all possible inputs correctly.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Default">The Color the pin is initialized with and can be reset to at any time.</param>
		/// <param name="HasAlpha">Hint to the GUI that this Color has an alpha channel.</param>
		void SetSubType(RGBAColor Default, bool HasAlpha);
	}
	
	#endregion color pins
	
	#region enum pins
	/// <summary>
	/// Interface to a ConfigurationPin of type Enum.
	/// </summary>
	[Guid("2FE17270-7B4C-4A46-A4EB-E8B56B9AD197"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumConfig: IPluginConfig
	{
		/// <summary>
		/// Used to write an Enum given as an ordinal value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Enum to.</param>
		/// <param name="Value">The ordinal Enum value to write.</param>
		void SetOrd(int Index, int Value);
		/// <summary>
		/// Used to write an Enum given as an string value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Enum to.</param>
		/// <param name="Value">The string Enum value to write.</param>
		void SetString(int Index, string Value);
		/// <summary>
		/// Used to retrieve an Enum in ordinal form from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="Value">The retrieved Enum.</param>
		void GetOrd(int Index, out int Value);
		/// <summary>
		/// Used to retrieve an Enum in string form from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="Value">The retrieved Enum.</param>
		void GetString(int Index, out string Value);
		/// <summary>
		/// Used to set the SubType of an Enum pin. Should only be called once immediately 
		/// after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="EnumName">Name of the Enum type to set to the pin. If the given name 
		/// is not yet registered with vvvv a new type with this name is created. 
		/// Using <see cref="VVVV.PluginInterfaces.V1.IPluginHost.UpdateEnum">IPluginHost.UpdateEnum</see> 
		/// a newly created Enum can be filled with custom entries.</param>
		void SetSubType(string EnumName);
	}
	
	/// <summary>
	/// Interface to an InputPin of type Enum.
	/// </summary>
	[Guid("DE852C36-FE3A-4767-97F5-7595A9A59D6A"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve an Enum in ordinal form from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="Value">The retrieved Enum.</param>
		void GetOrd(int Index, out int Value);
		/// <summary>
		/// Used to retrieve an Enum in string form from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Enum from.</param>
		/// <param name="Value">The retrieved Enum.</param>
		void GetString(int Index, out string Value);
		/// <summary>
		/// Used to set the SubType of an Enum pin. Should only be called once immediately 
		/// after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="EnumName">Name of the Enum type to set to the pin. If the given name 
		/// is not yet registered with vvvv a new type with this name is created. 
		/// Using <see cref="VVVV.PluginInterfaces.V1.IPluginHost.UpdateEnum">IPluginHost.UpdateEnum</see> 
		/// a newly created Enum can be filled with custom entries.</param>
		void SetSubType(string EnumName);
	}

	/// <summary>
	/// Interface to an OutputPin of type Enum.
	/// </summary>
	[Guid("C933059A-C46E-4149-966D-04D03B93A078"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumOut: IPluginOut
	{
		/// <summary>
		/// Used to write an Enum given as an ordinal value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Enum to.</param>
		/// <param name="Value">The ordinal Enum value to write.</param>
		void SetOrd(int Index, int Value);
		/// <summary>
		/// Used to write an Enum given as an string value to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Enum to.</param>
		/// <param name="Value">The string Enum value to write.</param>
		void SetString(int Index, string Value);
		/// <summary>
		/// Used to set the SubType of an Enum pin. Should only be called once immediately 
		/// after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="EnumName">Name of the Enum type to set to the pin. If the given name 
		/// is not yet registered with vvvv a new type with this name is created. 
		/// Using <see cref="VVVV.PluginInterfaces.V1.IPluginHost.UpdateEnum">IPluginHost.UpdateEnum</see> 
		/// a newly created Enum can be filled with custom entries.</param>
		void SetSubType(string EnumName);
	}
	#endregion enum pins
	
	#region node pins
	/// <summary>
	/// Base Interface for NodePin connections
	/// </summary>
	[Guid("AB312E34-8025-40F2-8241-1958793F3D39"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeIOBase
	{}
	
	/// <summary>
	/// Interface to an InputPin of the generic node type
	/// </summary>
	[Guid("FE6FEBC6-8581-4EB5-9AC8-E428CB9D1A03"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve the actual slice index this pin has to access on the upstream node. Note that the actual slice
		/// index maybe convoluted by an upstream node like GetSlice (node).
		/// </summary>
		/// <param name="slice">The slice index as seen by this pin.</param>
		/// <param name="UpstreamSlice">The actual slice index as probably convoluted via upstream GetSlice (node).</param>
		void GetUpsreamSlice(int slice, out int UpstreamSlice);
		/// <summary>
		/// Used to retrieve a reference of an interface offered by the upstream connected node.
		/// </summary>
		/// <param name="UpstreamInterface">The retrieved interface.</param>
		void GetUpstreamInterface(out INodeIOBase UpstreamInterface);
		/// <summary>
		/// Used to set the SubType of a node pin, which is a more detailed specification of the node type via a set of Guids that identifiy the interfaces accepted on this pin.
		/// The SubType is used by the GUI to guide the user to make only links between pins that understand the same interfaces.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Guids">An array of Guids (typically only one) that specifies the interfaces that this input accepts.</param>
		/// <param name="FriendlyName">A user readable name specifying the type of the node connection.</param>
		void SetSubType(Guid[] Guids, string FriendlyName);
	}
	
	/// <summary>
	/// Interface to an OutputPin of the generic node type
	/// </summary>
	[Guid("5D4F7524-CC1B-44FA-881F-A88D343D7A21"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeOut: IPluginOut
	{
		/// <summary>
		/// Used to set the interface this
		/// </summary>
		/// <param name="TheInterface"></param>
		void SetInterface(INodeIOBase TheInterface);
		/// <summary>
		/// Used to set the SubType of a node pin, which is a more detailed specification of the node type via a set of Guids that identifiy the interfaces offered on this pin.
		/// The SubType is used by the GUI to guide the user to make only links between pins that understand the same interfaces.
		/// Should only be called once immediately after the pin has been created in <see cref="VVVV.PluginInterfaces.V1.IPlugin.SetPluginHost()">IPlugin.SetPluginHost</see>.
		/// </summary>
		/// <param name="Guids">An array of Guids (typically only one) that specifies the interfaces that this output accepts.</param>
		/// <param name="FriendlyName">A user readable name specifying the type of the node connection.</param>
		void SetSubType(Guid[] Guids, string FriendlyName);
		/// <summary>
		/// Used to mark this pin as being changed compared to the last frame. 
		/// </summary>
		void MarkPinAsChanged();
	}
	
	/// <summary>
	/// Interface to an InputPin of type Transform.
	/// </summary>
	[Guid("605FD0B2-AD68-40B4-92E5-819599544CF2"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITransformIn: IPluginIn
	{
		/// <summary>
		/// Used to retrieve a Matrix from the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Matrix from.</param>
		/// <param name="Value">The retrieved Matrix.</param>
		void GetMatrix(int Index, out Matrix4x4 Value);
		/// <summary>
		/// Used to retrieve a World Matrix from the pin at the specified slice. You should call this method only from within your Render method when supporting the IPluginDXLayer interface.
		/// </summary>
		/// <param name="Index">The index of the slice to retrieve the Matrix from.</param>
		/// <param name="Value">The retrieved Matrix.</param>
		void GetRenderWorldMatrix(int Index, out Matrix4x4 Value);
		/// <summary>
		/// Used to initialize rendering by letting vvvv know which transform pin controls spaces. This sets view and projection matrices.
		/// </summary>
		void SetRenderSpace();
	}
	
	/// <summary>
	/// Interface to an OutputPin of type Transform.
	/// </summary>
	[Guid("AA8D6410-36E5-4EA2-AF70-66CD6321FF36"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ITransformOut: IPluginOut
	{
		/// <summary>
		/// Used to write a Matrix to the pin at the specified slice.
		/// </summary>
		/// <param name="Index">The index of the slice to write the Matrix to.</param>
		/// <param name="Value">The Matrix to write.</param>
		void SetMatrix(int Index, Matrix4x4 Value);
	}
	#endregion node pins
	
	#region host, plugin
	
	/// <summary>
	/// The interface to be implemented by a program to host IPlugins
	/// </summary>
	[Guid("E72C5CF0-4738-4F20-948E-83E96D4E7843"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginHost
	{
		/// <summary>
		/// Creates a ConfigurationPin of type Value.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="dimension">The pins dimension count. Valid values: 1, 2, 3 or 4</param>
		/// <param name="DimensionNames">Optional. An individual suffix to the pins Dimensions.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="visibility">The pins initial visibility.</param>
		/// <param name="pin">Pointer to the created IValueConfig interface.</param>
		void CreateValueConfig(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueConfig Pin);
		/// <summary>
		/// Creates an InputPin of type Value. Use this as opposed to <see cref="VVVV.PluginInterfaces.V1.IPluginHost.CreateValueFastInput()">CreateValueFastInput</see>
		/// if you need to be able to ask for <see cref="VVVV.PluginInterfaces.V1.IPluginIn.PinIsChanged">IPluginIn.PinIsChanged</see>. May be slow with large SpreadCounts.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Dimension">The pins dimension count. Valid values: 1, 2, 3 or 4</param>
		/// <param name="DimensionNames">Optional. An individual suffix to the pins Dimensions.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IValueIn interface.</param>
		void CreateValueInput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueIn Pin);
		/// <summary>
		/// Creates an InputPin of type Value that does not implement <see cref="VVVV.PluginInterfaces.V1.IPluginIn.PinIsChanged">IPluginIn.PinIsChanged</see> and is therefore faster with large SpreadCounts.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Dimension">The pins dimension count. Valid values: 1, 2, 3 or 4</param>
		/// <param name="DimensionNames">Optional. An individual suffix to the pins Dimensions.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IValueFastIn interface.</param>
		void CreateValueFastInput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueFastIn Pin);
		/// <summary>
		/// Creates an OutputPin of type Value.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Dimension">The pins dimension count. Valid values: 1, 2, 3 or 4</param>
		/// <param name="DimensionNames">Optional. An individual suffix to the pins Dimensions.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IValueOut interface.</param>
		void CreateValueOutput(string Name, int Dimension, string[] DimensionNames, TSliceMode SliceMode, TPinVisibility Visibility, out IValueOut Pin);
		/// <summary>
		/// Creates a ConfigurationPin of type String.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IStringConfig interface.</param>
		void CreateStringConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringConfig Pin);
		/// <summary>
		/// Creates an InputPin of type String.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IStringIn interface.</param>
		void CreateStringInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringIn Pin);
		/// <summary>
		/// Creates an OutputPin of type String.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IStringIn interface.</param>
		void CreateStringOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IStringOut Pin);
		/// <summary>
		/// Creates a ConfigurationPin of type Color.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IColorConfig interface.</param>
		void CreateColorConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorConfig Pin);
		/// <summary>
		/// Creates an InputPin of type Color.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IColorIn interface.</param>
		void CreateColorInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorIn Pin);
		/// <summary>
		/// Creates an OutputPin of type Color.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IColorOut interface.</param>
		void CreateColorOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IColorOut Pin);
		/// <summary>
		/// Creates a ConfigurationPin of type Enum.
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IEnumConfig interface.</param>
		void CreateEnumConfig(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IEnumConfig Pin);			
		/// <summary>
		/// Creates a InputPin of type Enum.
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IEnumIn interface.</param>
		void CreateEnumInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IEnumIn Pin);
		/// <summary>
		/// Creates a OutputPin of type Enum.
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IEnumOut interface.</param>
		void CreateEnumOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IEnumOut Pin);
		/// <summary>
		/// Creates an InputPin of type Transform.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created ITransformIn interface.</param>
		void CreateTransformInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out ITransformIn Pin);
		/// <summary>
		/// Creates an OutputPin of type Transform.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created ITransformOut interface.</param>
		void CreateTransformOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out ITransformOut Pin);
		/// <summary>
		/// Creates an InputPin of the generic node type.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created INodeIn interface.</param>
		void CreateNodeInput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out INodeIn Pin);
		/// <summary>
		/// Creates an OutputPin of the generic node type.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created INodeIn interface.</param>
		void CreateNodeOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out INodeOut Pin);
		/// <summary>
		/// Creates an OutputPin of type DirectX Mesh.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IDXMeshIO interface.</param>
		void CreateMeshOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IDXMeshOut Pin);
		/// <summary>
		/// Creates an OutputPin of type DirectX Texture.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="SliceMode">The pins SliceMode.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IDXTextureOut interface.</param>
		void CreateTextureOutput(string Name, TSliceMode SliceMode, TPinVisibility Visibility, out IDXTextureOut Pin);
		/// <summary>
		/// Creates an OutputPin of type DirectX Layer.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IDXLayerIO interface.</param>
		void CreateLayerOutput(string Name, TPinVisibility Visibility, out IDXLayerIO Pin);
		/// <summary>
		/// Creates an InputPin of type DirectX RenderState.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IDXRenderStateIO interface.</param>
		void CreateRenderStateInput(TSliceMode SliceMode, TPinVisibility Visibility, out IDXRenderStateIn Pin);
		/// <summary>
		/// Creates an InputPin of type DirectX SamplerState.
		/// </summary>
		/// <param name="Name">The pins name.</param>
		/// <param name="Visibility">The pins initial visibility.</param>
		/// <param name="Pin">Pointer to the created IDXRenderStateIO interface.</param>
		void CreateSamplerStateInput(TSliceMode SliceMode, TPinVisibility Visibility, out IDXSamplerStateIn Pin);
		/// <summary>
		/// Deletes the given pin from the plugin
		/// </summary>
		/// <param name="Pin">The pin to be deleted</param>
		void DeletePin(IPluginIO Pin);
		/// <summary>
		/// Returns the current time which the plugin should use if it does timebased calculations.
		/// </summary>
		/// <param name="CurrentTime">The hosts current time.</param>
		void GetCurrentTime(out double CurrentTime);
		/// <summary>
		/// Returns the absolut file path to the plugins host.
		/// </summary>
		/// <param name="Path">Absolut file path to the plugins host (i.e path to the patch the plugin is placed in, in vvvv).</param>
		void GetHostPath(out string Path);
		/// <summary>
		/// Returns a slash-separated path of node IDs that uniquely identifies this node in the vvvv graph.
		/// </summary>
		/// <param name="UseDescriptiveNames">If TRUE descriptive node names are used where available instead of the node ID.</param>
		/// <param name="Path">Slash-separated path of node IDs that uniquely identifies this node in the vvvv graph.</param>
		void GetNodePath(bool UseDescriptiveNames, out string Path);
		/// <summary>
		/// Allows a plugin to write messages to a console on the host (ie. Renderer (TTY) in vvvv).
		/// </summary>
		/// <param name="Type">The type of message. Depending on the setting of this parameter the PluginHost can handle messages differently.</param>
		/// <param name="Message">The message to be logged.</param>
		void Log(TLogType Type, string Message);
		/// <summary>
		/// Allows a plugin to create/update an Enum with vvvv
		/// </summary>
		/// <param name="EnumName">The Enums name.</param>
		/// <param name="Default">The Enums default value.</param>
		/// <param name="EnumEntries">An array of strings that specify the enums entries.</param>
		void UpdateEnum(string EnumName, string Default, string[] EnumEntries);
		/// <summary>
		/// Returns the number of entries for a given Enum.
		/// </summary>
		/// <param name="EnumName">The name of the Enum to get the EntryCount of.</param>
		/// <param name="EntryCount">Number of entries in the Enum.</param>
		void GetEnumEntryCount(string EnumName, out int EntryCount);
		/// <summary>
		/// Returns the name of a given EnumEntry of a given Enum.
		/// </summary>
		/// <param name="EnumName">The name of the Enum to get the EntryName of.</param>
		/// <param name="Index">Index of the EnumEntry.</param>
		/// <param name="EntryName">String representation of the EnumEntry.</param>
		void GetEnumEntry(string EnumName, int Index, out string EntryName);
	}
	
	/// <summary>
	/// The one single interface a plugin has to implement
	/// </summary>
	[Guid("7F813C89-4EDE-4087-A626-4320BE41C87F"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPlugin
	{
		/// <summary>
		/// Called by the PluginHost to hand itself over to the plugin. This is where the plugin creates its initial pins.
		/// </summary>
		/// <param name="Host">Interface to the PluginHost.</param>
		void SetPluginHost(IPluginHost Host);
		/// <summary>
		/// Called by the PluginHost before the Evaluate function every frame for every ConfigurationPin that has changed. 
		/// The ConfigurationPin is handed over as the functions input parameter. This is where a plugin would typically 
		/// create/delete pins as reaction to the changed value of a ConfigurationPin that specifies the number of pins of a specific type.
		/// </summary>
		/// <param name="input">Interface to the ConfigurationPin for which the function is called.</param>
		void Configurate(IPluginConfig input);
		/// <summary>
		/// Called by the PluginHost once per frame. This is where the plugin calculates and sets the SliceCounts and Values
		/// of its outputs depending on the values of its current inputs.
		/// </summary>
		/// <param name="SpreadMax">The maximum SliceCount of all of the plugins inputs, which would typically be used
		/// to adjust the SliceCounts of all outputs accordingly.</param>
		void Evaluate(int SpreadMax);
		/// <summary>
		/// Called by the PluginHost only once during initialization to find out if this plugin needs to be evaluated
		/// every frame even if there is not output connected. Typically this can return FALSE as long as the plugin doesn't have
		/// a special reason for doing otherwise.
		/// </summary>
		bool AutoEvaluate {get;}
	}
	
	/// <summary>
	/// Optional interface to be implemented on a plugin that needs to know when one of its pins is connected or disconnected
	/// </summary>
	[Guid("B77C459E-E561-424B-AB3A-572C9BB6CD93"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginConnections
	{
		/// <summary>
		/// Called by the PluginHost for every input or output that is being connected. This is typically useful for 
		/// NodeIO Inputs that can cache a reference to the upstream interface at this place instead of getting the reference
		/// every frame in Evaluate.
		/// </summary>
		/// <param name="pin">Interface to the pin for which the function is called.</param>
		void ConnectPin(IPluginIO pin);
		/// <summary>
		/// Called by the PluginHost for every input or output that is being disconnected. This is typically useful for 
		/// NodeIO Inputs that can set a cached reference to the upstream interface to null at this place.
		/// </summary>
		/// <param name="pin">Interface to the pin for which the function is called.</param>
		void DisconnectPin(IPluginIO pin);
	}

	#endregion host, plugin
	
	#region plugin info
	
	/// <summary>
	/// Interface for the <see cref="VVVV.PluginInterfaces.V1.PluginInfo">PluginInfo</see>. Also see <a href="http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming" target="_blank">VVVV Naming Conventions</a>.
	/// </summary>
	[Guid("16EE5CF9-0D75-4ECF-9440-7D2909E8F7DC"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPluginInfo
	{
		/// <summary>
		/// The nodes main visible name. Use CamelCaps and no spaces.
		/// </summary>
		string Name {get; set;}
		/// <summary>
		/// The category in which the plugin can be found. Try to use an existing one.
		/// </summary>
		string Category {get; set;}
		/// <summary>
		/// Optional. Leave blank if not needed to distinguish two nodes of the same name and category.
		/// </summary>
		string Version {get; set;}
		/// <summary>
		/// Describe the nodes function in a few words.
		/// </summary>
		string Help {get; set;}
		/// <summary>
		/// Specify a comma separated list of tags that describe the node. Name, category and Version don't need to be duplicated here.
		/// </summary>
		string Tags {get; set;}
		/// <summary>
		/// Specify the plugins author.
		/// </summary>
		string Author {get; set;}
		/// <summary>
		/// Give credits to thirdparty code used.
		/// </summary>
		string Credits {get; set;}
		/// <summary>
		/// Specify known problems.
		/// </summary>
		string Bugs {get; set;}
		/// <summary>
		/// Specify any usage of the node that may cause troubles.
		/// </summary>
		string Warnings {get; set;}
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial size in window-mode.
		/// </summary>
		Size InitialWindowSize {get; set;}
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial size in box-mode.
		/// </summary>
		Size InitialBoxSize {get; set;}
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial component mode.
		/// </summary>
		TComponentMode InitialComponentMode {get; set;}
		
		/// <summary>
		/// The nodes namespace. Filled out automatically, when using code as seen in the PluginTemplate.
		/// </summary>
		string Namespace {get; set;}
		/// <summary>
		/// The nodes classname. Filled out automatically, when using code as seen in the PluginTemplate.
		/// </summary>
		string Class {get; set;}
	}
	
	/// <summary>
	/// Interface for the <see cref="VVVV.PluginInterfaces.V1.INodeInfo">INodeInfo</see>. Also see <a href="http://www.vvvv.org/tiki-index.php?page=Conventions.NodeAndPinNaming" target="_blank">VVVV Naming Conventions</a>.
	/// </summary>
	[Guid("581998D6-ED08-4E73-821A-46AFF59C78BD"),
	 InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface INodeInfo : IPluginInfo
	{
		/// <summary>
		/// Arguments used by the PluginFactory to create this node.
		/// </summary>
		string Arguments {get; set;}
		/// <summary>
		/// Name of the file used by the PluginFactory to create this node.
		/// </summary>
		string Filename {get; set;}
		/// <summary>
		/// The node type. Set by the PluginFactory.
		/// </summary>
		TNodeType Type {get; set;}
		/// <summary>
		/// Reference to the <see cref="VVVV.HDE.Interfaces.IExecutable">IExecutable</see> which was used to create this node. Set by the PluginFactory.
		/// </summary>
		IExecutable Executable {get; set;}
	}
	
	/// <summary>
	/// Helper Class that implements the <see cref="VVVV.PluginInterfaces.V1.IPluginInfo">IPluginInfo</see> interface.
	/// </summary>
	[Guid("FE1216D6-5439-416D-8FB7-16E9A29EF67B")]
	public class PluginInfo: MarshalByRefObject, IPluginInfo
	{
		private string FName = "";
		private string FCategory = "";
		private string FVersion = "";
		private string FAuthor = "";
		private string FHelp = "";
		private string FTags = "";
		private string FBugs = "";
		private string FCredits = "";
		private string FWarnings = "";
		private string FNamespace = "";
		private string FClass = "";
		private Size FInitialWindowSize = new Size(0, 0);
		private Size FInitialBoxSize = new Size(0, 0);
		private TComponentMode FInitialComponentMode = TComponentMode.Hidden;
		
		/// <summary>
		/// The nodes main visible name. Use CamelCaps and no spaces.
		/// </summary>
		public string Name
		{
			get {return FName;}
			set {FName = value;}
		}
		/// <summary>
		/// The category in which the plugin can be found. Try to use an existing one.
		/// </summary>
		public string Category
		{
			get {return FCategory;}
			set {FCategory = value;}
		}
		/// <summary>
		/// Optional. Leave blank if not needed to distinguish two nodes of the same name and category.
		/// </summary>
		public string Version
		{
			get {return FVersion;}
			set {FVersion = value;}
		}
		/// <summary>
		/// Specify the plugins author.
		/// </summary>
		public string Author
		{
			get {return FAuthor;}
			set {FAuthor = value;}
		}
		/// <summary>
		/// Describe the nodes function in a few words.
		/// </summary>
		public string Help
		{
			get {return FHelp;}
			set {FHelp = value;}
		}
		/// <summary>
		/// Specify a comma separated list of tags that describe the node. Name, category and Version don't need to be duplicated here.
		/// </summary>
		public string Tags
		{
			get {return FTags;}
			set {FTags = value;}
		}
		/// <summary>
		/// Specify known problems.
		/// </summary>
		public string Bugs
		{
			get {return FBugs;}
			set {FBugs = value;}
		}
		/// <summary>
		/// Give credits to thirdparty code used.
		/// </summary>
		public string Credits
		{
			get {return FCredits;}
			set {FCredits = value;}
		}
		/// <summary>
		/// Specify any usage of the node that may cause troubles.
		/// </summary>
		public string Warnings
		{
			get {return FWarnings;}
			set {FWarnings = value;}
		}
		/// <summary>
		/// The nodes namespace. Filled out automatically, when using code as seen in the PluginTemplate.
		/// </summary>
		public string Namespace
		{
			get {return FNamespace;}
			set {FNamespace = value;}
		}
		/// <summary>
		/// The nodes classname. Filled out automatically, when using code as seen in the PluginTemplate.
		/// </summary>
		public string Class
		{
			get {return FClass;}
			set {FClass = value;}
		}
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial size in window-mode.
		/// </summary>
		public Size InitialWindowSize
		{
			get {return FInitialWindowSize;}
			set {FInitialWindowSize = value;}
		}
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial size in box-mode.
		/// </summary>
		public Size InitialBoxSize
		{
			get {return FInitialBoxSize;}
			set {FInitialBoxSize = value;}
		}
		
		/// <summary>
		/// Only for GUI plugins. Defines the nodes initial component mode.
		/// </summary>
		public TComponentMode InitialComponentMode
		{
			get {return FInitialComponentMode;}
			set {FInitialComponentMode = value;}
		}
	}
	
	/// <summary>
	/// Helper Class that implements the <see cref="VVVV.PluginInterfaces.V1.INodeInfo">INodeInfo</see> interface.
	/// </summary>
	[Guid("36F845F4-A486-49EC-9A0C-CB254FF2B297")]
	public class NodeInfo: PluginInfo, INodeInfo
	{
		
		private string FArguments = "";
		private string FFilename = "";
		private TNodeType FType = TNodeType.Plugin;
		private IExecutable FExcecutable = null;
		
		/// <summary>
		/// Arguments used by the PluginFactory to create this node.
		/// </summary>
		public string Arguments
		{
			get {return FArguments;}
			set {FArguments = value;}
		}
		
		/// <summary>
		/// Name of the file used by the PluginFactory to create this node.
		/// </summary>
		public string Filename 
		{
			get {return FFilename;}
			set {FFilename = value;}
		}
		
		/// <summary>
		/// The node type. Set by the PluginFactory.
		/// </summary>
		public TNodeType Type 
		{
			get {return FType;}
			set {FType = value;}
		}
		
		/// <summary>
		/// Reference to the <see cref="VVVV.HDE.Interfaces.IExecutable">IExecutable</see> which was used to create this node. Set by the PluginFactory.
		/// </summary>
		public IExecutable Executable 
		{
			get {return FExcecutable;}
			set {FExcecutable = value;}
		}
	}
	
	#endregion plugin info
	
}
