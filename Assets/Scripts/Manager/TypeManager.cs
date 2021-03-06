//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1008
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class TypeManager
{
	private static TypeManager instance = null;

	private string TypeFile = "Config/Type.txt";

	private List<TypeItem> m_TypeItemList = new List<TypeItem>();

	public static TypeManager Instance
	{
		get
		{
			if (instance == null)
				instance  = new TypeManager();

			return instance;
		}
	}

	public List<TypeItem> TypeItemList
	{
		get { return m_TypeItemList; }
	}

	public TypeManager ()
	{
		LoadDataFromTxt(TypeFile);
	}

	void LoadDataFromTxt(string fileName)
	{
		TextAsset ta = Resources.Load(fileName.Split('.')[0]) as TextAsset;
		StringReader sr = new StringReader(ta.text);

		Debug.Log(ta.text);

		string line = "";
		string[] lineData;

		// pass first and second
		line = sr.ReadLine();
		line = sr.ReadLine();
		
		TypeItem ti;

		line = sr.ReadLine();
		while( line != null)
		{
			lineData = line.Split('	');
			ti = new TypeItem();
			ti.ID = int.Parse(lineData[0]);
			ti.TypeName = lineData[1];

			for (int i = 2; i < lineData.Length; i++)
			{
				ti.TypeList.Add(lineData[i]);
			}

			m_TypeItemList.Add(ti);

			line = sr.ReadLine();
		}
		
		sr.Close();

	}

	public List<string> GetTypeListByName(string name)
	{
		foreach (TypeItem ti in m_TypeItemList)
		{
			if (ti.TypeName.Equals(name))
			{
				return ti.TypeList;
			}
		}

		return null;
	}
}


