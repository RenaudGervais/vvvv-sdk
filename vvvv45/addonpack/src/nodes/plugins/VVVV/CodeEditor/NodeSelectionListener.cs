﻿using System;
using VVVV.PluginInterfaces.V1;
using VVVV.Core.Model;

namespace VVVV.HDE.CodeEditor
{
	public class NodeSelectionListener : INodeSelectionListener
	{
		private CodeEditorPlugin FCodeEditorPlugin;
		
		public NodeSelectionListener(CodeEditorPlugin codeEditorPlugin)
		{
			FCodeEditorPlugin = codeEditorPlugin;
		}
		
		public void NodeSelectionChangedCB(INode[] nodes)
		{
			foreach (var node in nodes)
			{
				var nodeInfo = node.GetNodeInfo();
				var executable = nodeInfo.Executable;
				
				if (executable == null)
					continue;
				
				var project = executable.Project;
				
				if (project == null)
					continue;
				
				foreach (var doc in project.Documents)
				{
					if (doc is ITextDocument)
					{
						// TODO: FCodeEditorPlugin.Open(doc as ITextDocument, nodeInfo);
						FCodeEditorPlugin.Open(doc as ITextDocument);
					}
				}
			}
		}
	}
}
