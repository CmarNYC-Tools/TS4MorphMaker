/* TS4 MorphMaker, a tool for creating custom content for The Sims 4,
   Copyright (C) 2014  C. Marinetti

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>. 
   The author may be contacted at modthesims.info, username cmarNYC. */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using s4pi.Interfaces;
using s4pi.Package;

namespace MorphTool
{
    public partial class Form1 : Form
    {
        TreeNode currentNode;
        Color foreColor;

        private void MorphTreeSetup()
        {
            foreColor = morphs_treeView.ForeColor;
        }

        private void morphs_treeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (currentNode != null)
            {
                currentNode.ForeColor = foreColor;
                currentNode.BackColor = Color.Transparent;
            }
            currentNode = e.Node;
            currentNode.ForeColor = Color.Blue;
            currentNode.BackColor = Color.LightCyan;
            if (e.Action == TreeViewAction.Collapse || e.Action == TreeViewAction.Expand) return;
            morphs_treeView.SelectedNode = null;

        }

        private void ShowTreeView()
        {
            morphs_treeView.Nodes.Clear();
            string[] sliderDirections = new string[] { "Left", "Right", "Up", "Down" };

            for (int i = 0; i < hotcList.Count; i++)
            {
                HOTC hotc = hotcList[i];
                string hotcName = hotc.HotSpotTGI.ToString();
                string hotcText = hotc.HotSpotName;
                TreeNode tmph = new TreeNode("HotSpotControl: " + hotcText);
                tmph.Name = hotcName;
                morphs_treeView.Nodes.Add(tmph);

                List<HOTC.Slider> sliders = hotc.sliderDescriptions;
                for (int s = 0; s < sliders.Count; s++)
                {
                    string sliderName = hotcName + s.ToString();
                    string sliderText = sliders[s].Angle.ToString();
                    TreeNode tmpSlider = new TreeNode(sliderText);
                    tmpSlider.Name = sliderName;
                    morphs_treeView.Nodes[hotcName].Nodes.Add(tmpSlider);
                    for (int j = 0; j < sliders[s].SimModifierInstances.Length; j++)
                    {
                        TreeNode tmpSmod = GetSMOD_Node(sliders[s].SimModifierInstances[j], sliderName + sliderDirections[j], sliderDirections[j] + ": ", nmap);
                        morphs_treeView.Nodes[hotcName].Nodes[sliderName].Nodes.Add(tmpSmod);
                    }                    
                }
            }
            for (int i = 0; i < presetList.Count; i++)
            {
                CPRE cpre = presetList[i];
                string cpreName = cpre.PresetTGI.ToString();
                string cpreText = cpre.PresetName;
                TreeNode tmpp = new TreeNode("Preset: " + cpreText);
                tmpp.Name = cpreName;
                morphs_treeView.Nodes.Add(tmpp);

                CPRE.SculptLink[] sculpts = cpre.sculpts;
                for (int s = 0; s < sculpts.Length; s++)
                {
                    string sculptName = cpreName + "Sculpt" + s.ToString();
                    string sculptText = "Sculpt" + s.ToString();
                    int ind = ListKeyLookUp(sculptList, sculpts[s].instance);
                    if (ind < 0)
                    {
                        Predicate<IResourceIndexEntry> predSculpt = r => r.ResourceType == (uint)ResourceTypes.Sculpt &
                                r.ResourceGroup == 0x00000000U & r.Instance == sculpts[s].instance;
                        sculptText += IsInGamePacks(predSculpt) ? " (EA)" : " (Not found)";
                    }
                    else
                    {
                        sculptText += ": " + sculptList[ind].SculptName;
                    }
                    TreeNode tmpSculpt = new TreeNode(sculptText);
                    tmpSculpt.Name = sculptName;
                    morphs_treeView.Nodes[cpreName].Nodes.Add(tmpSculpt);
                }

                CPRE.Modifier[] modifiers = cpre.modifiers;
                for (int m = 0; m < modifiers.Length; m++)
                {
                    string modName = cpreName + "Modifier" + m.ToString();
                    string modText = "Modifier" + m.ToString();
                    TreeNode tmpModifier = new TreeNode(modText);
                    tmpModifier.Name = modName;
                    morphs_treeView.Nodes[cpreName].Nodes.Add(tmpModifier);
                    TreeNode tmpSmod = GetSMOD_Node(modifiers[m].instance, modName + m.ToString(), "", nmap);
                    morphs_treeView.Nodes[cpreName].Nodes[modName].Nodes.Add(tmpSmod);
                }
            }
            morphs_treeView.ExpandAll();
        }

        private TreeNode GetSMOD_Node(ulong instance, string name, string textPrefix, NameMap nmap)
        {
            if (instance == 0)
            {
                TreeNode emptySmod = new TreeNode(textPrefix + "None");
                return emptySmod;
            }
            string smodName = name;
            string smodText = null;
            bool dummy;
            smodText = GetSmodName(instance, out dummy);
            TreeNode tmpSmod = new TreeNode(textPrefix + smodText);
            tmpSmod.Name = smodName;
            return tmpSmod;
        }
    }
}
