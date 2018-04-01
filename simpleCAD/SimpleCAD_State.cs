using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Runtime.Serialization;

namespace simpleCAD
{
	//=============================================================================
	/// <summary>
	/// State of SimpleCAD.
	/// 
	/// Responsobilities:
	/// - Helper class for serialization\deserialization from file.
	/// - part of implementation "Memento" pattern
	/// </summary>
	[Serializable]
	public class SimpleCAD_State : ISerializable
	{
		public SimpleCAD_State(
			List<ICadGeometry> geometries,
			Vector offset,
			ICadGeometry geometryToCreate,
			Color axesColor,
			double rAxesThickness,
			double rAxesLength,
			double rAxesTextSize,
			double rScale)
		{
			Geometries = geometries;
			OffsetVector = offset;
			GeometryToCreate = geometryToCreate;
			AxesColor = axesColor;
			AxesThickness = rAxesThickness;
			AxesLength = rAxesLength;
			AxesTextSize = rAxesTextSize;
			Scale = rScale;
		}

		public List<ICadGeometry> Geometries { get; private set; }
		public Vector OffsetVector { get; private set; }

		public ICadGeometry GeometryToCreate { get; private set; }

		public Color AxesColor { get; private set; }
		public double AxesThickness { get; private set; }
		public double AxesLength { get; private set; }
		public double AxesTextSize { get; private set; }

		public double Scale { get; private set; }

		//=============================================================================
		// Implement this method to serialize data. The method is called 
		// on serialization.
		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("Geometries", Geometries);
			info.AddValue("OffsetVector", OffsetVector);

			info.AddValue("GeometryToCreate", GeometryToCreate);

			info.AddValue("AxesColor", AxesColor.ToString());
			info.AddValue("AxesThickness", AxesThickness);
			info.AddValue("AxesLength", AxesLength);
			info.AddValue("AxesTextSize", AxesTextSize);

			info.AddValue("Scale", Scale);
		}

		//=============================================================================
		// The special constructor is used to deserialize values.
		public SimpleCAD_State(SerializationInfo info, StreamingContext context)
		{
			Geometries = (List<ICadGeometry>)info.GetValue("Geometries", typeof(List<ICadGeometry>));
			OffsetVector = (Vector)info.GetValue("OffsetVector", typeof(Vector));

			GeometryToCreate = (ICadGeometry)info.GetValue("GeometryToCreate", typeof(ICadGeometry));

			try
			{
				AxesColor = (Color)ColorConverter.ConvertFromString((string)info.GetValue("AxesColor", typeof(string)));
			}
			catch
			{
				AxesColor = Colors.Black;
			}

			AxesThickness = (double)info.GetValue("AxesThickness", typeof(double));
			AxesLength = (double)info.GetValue("AxesLength", typeof(double));
			AxesTextSize = (double)info.GetValue("AxesTextSize", typeof(double));

			Scale = (double)info.GetValue("Scale", typeof(double));
		}
	}

	[Serializable]
	public class DefaultState : SimpleCAD_State
	{
		public DefaultState(double rControlWidth, double rControlHeight)
			: base(
				new List<ICadGeometry>(),
				new Vector(-rControlWidth/2, rControlHeight/2),
				null,
				Colors.Black,
				2.0,
				50.0,
				12.0,
				1)
		{ }
	}
}
