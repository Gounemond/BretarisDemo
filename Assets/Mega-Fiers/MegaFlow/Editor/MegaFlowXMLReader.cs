
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MegaFlowXMLValue
{
	public string	name;
	public string	value;
}

public class MegaFlowXMLNode
{
	public String					tagName;
	public MegaFlowXMLNode			parentNode;
	public List<MegaFlowXMLNode>	children;
	public List<MegaFlowXMLValue>	values;

	public MegaFlowXMLNode()
	{
		tagName = "NONE";
		parentNode = null;
		children = new List<MegaFlowXMLNode>();
		values = new List<MegaFlowXMLValue>();
	}
}

public class MegaFlowXMLReader
{
	private static char TAG_START = '<';
	private static char TAG_END = '>';
	private static char SPACE = ' ';
	private static char QUOTE = '"';
	private static char SLASH = '/';
	private static char EQUALS = '=';
	private static String BEGIN_QUOTE = "" + EQUALS + QUOTE;

	public MegaFlowXMLReader()
	{
	}

	public MegaFlowXMLNode read(String xml)
	{
		int index = 0;
		int lastIndex = 0;
		MegaFlowXMLNode rootNode = new MegaFlowXMLNode();
		MegaFlowXMLNode currentNode = rootNode;

		xml = xml.Replace(" \n", "");
		xml = xml.Replace("\n", "");

		while ( true )
		{
			index = xml.IndexOf(TAG_START, lastIndex);

			if ( index < 0 || index >= xml.Length )
				break;

			index++;

			lastIndex = xml.IndexOf(TAG_END, index);
			if ( lastIndex < 0 || lastIndex >= xml.Length )
				break;

			int tagLength = lastIndex - index;
			String xmlTag = xml.Substring(index, tagLength);

			if ( xmlTag[0] == SLASH )
			{
				currentNode = currentNode.parentNode;
				continue;
			}

			bool openTag = true;

			if ( xmlTag[tagLength - 1] == SLASH )
			{
				xmlTag = xmlTag.Substring(0, tagLength - 1);
				openTag = false;
			}


			MegaFlowXMLNode node = parseTag(xmlTag);
			node.parentNode = currentNode;
			currentNode.children.Add(node);

			if ( openTag )
				currentNode = node;
		}

		return rootNode;
	}

	public MegaFlowXMLNode parseTag(String xmlTag)
	{
		MegaFlowXMLNode node = new MegaFlowXMLNode();

		int nameEnd = xmlTag.IndexOf(SPACE, 0);
		if ( nameEnd < 0 )
		{
			node.tagName = xmlTag;
			return node;
		}

		String tagName = xmlTag.Substring(0, nameEnd);
		node.tagName = tagName;

		String attrString = xmlTag.Substring(nameEnd, xmlTag.Length - nameEnd);
		return parseAttributes(attrString, node);
	}

	public MegaFlowXMLNode parseAttributes(String xmlTag, MegaFlowXMLNode node)
	{
		int index = 0;
		int attrNameIndex = 0;
		int lastIndex = 0;

		while ( true )
		{
			index = xmlTag.IndexOf(BEGIN_QUOTE, lastIndex);
			if ( index < 0 || index > xmlTag.Length )
				break;

			attrNameIndex = xmlTag.LastIndexOf(SPACE, index);
			if ( attrNameIndex < 0 || attrNameIndex > xmlTag.Length )
				break;

			attrNameIndex++;
			String attrName = xmlTag.Substring(attrNameIndex, index - attrNameIndex);

			index += 2;

			lastIndex = xmlTag.IndexOf(QUOTE, index);
			if ( lastIndex < 0 || lastIndex > xmlTag.Length )
			{
				break;
			}

			int tagLength = lastIndex - index;
			String attrValue = xmlTag.Substring(index, tagLength);

			MegaFlowXMLValue val = new MegaFlowXMLValue();
			val.name = attrName;
			val.value = attrValue;
			node.values.Add(val);
		}

		return node;
	}

	static char[] commaspace = new char[] { ',', ' ' };

	static public Vector3 ParseV3Split(string str, int i)
	{
		return ParseV3(str.Split(commaspace, StringSplitOptions.RemoveEmptyEntries), i);
	}

	static public Vector3 ParseV3(string[] str, int i)
	{
		Vector3 p = Vector3.zero;

		p.x = float.Parse(str[i]);
		p.y = float.Parse(str[i + 1]);
		p.z = float.Parse(str[i + 2]);
		return p;
	}
}
