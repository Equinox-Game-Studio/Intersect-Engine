﻿using System;
using System.Windows.Forms;
using Intersect.Editor.Localization;
using Intersect.Enums;
using Intersect.GameObjects;
using Intersect.GameObjects.Events;
using Intersect.GameObjects.Events.Commands;

namespace Intersect.Editor.Forms.Editors.Events.Event_Commands
{
    public partial class EventCommandVariable : UserControl
    {
        private readonly FrmEvent mEventEditor;
        private bool mLoading;
        private SetVariableCommand mMyCommand;

        public EventCommandVariable(SetVariableCommand refCommand, FrmEvent editor)
        {
            InitializeComponent();
            mMyCommand = refCommand;
            mEventEditor = editor;
            mLoading = true;
            InitLocalization();

            //Numerics
            cmbNumericCloneGlobalVar.Items.Clear();
            cmbNumericCloneGlobalVar.Items.AddRange(ServerVariableBase.Names);
            cmbNumericClonePlayerVar.Items.Clear();
            cmbNumericClonePlayerVar.Items.AddRange(PlayerVariableBase.Names);

            //Booleans
            cmbBooleanCloneGlobalVar.Items.Clear();
            cmbBooleanCloneGlobalVar.Items.AddRange(ServerVariableBase.Names);
            cmbBooleanClonePlayerVar.Items.Clear();
            cmbBooleanClonePlayerVar.Items.AddRange(PlayerVariableBase.Names);

            //Strings
            cmbDupGlobalString.Items.Clear();
            cmbDupGlobalString.Items.AddRange(ServerVariableBase.Names);
            cmbDupPlayerString.Items.Clear();
            cmbDupPlayerString.Items.AddRange(PlayerVariableBase.Names);

            if (mMyCommand.VariableType == VariableTypes.ServerVariable)
            {
                rdoGlobalVariable.Checked = true;
            }
            else
            {
                rdoPlayerVariable.Checked = true;
            }
            mLoading = false;
            InitEditor();
        }

        private void InitLocalization()
        {
            grpSetVariable.Text = Strings.EventSetVariable.title;
           
            grpSelectVariable.Text = Strings.EventSetVariable.label;
            rdoGlobalVariable.Text = Strings.EventSetVariable.global;
            rdoPlayerVariable.Text = Strings.EventSetVariable.player;
            btnSave.Text = Strings.EventSetVariable.okay;
            btnCancel.Text = Strings.EventSetVariable.cancel;
            chkSyncParty.Text = Strings.EventSetVariable.syncparty;

            //Numeric
            grpNumericVariable.Text = Strings.EventSetVariable.numericlabel;
            grpNumericRandom.Text = Strings.EventSetVariable.numericrandomdesc;
            optNumericSet.Text = Strings.EventSetVariable.numericset;
            optNumericAdd.Text = Strings.EventSetVariable.numericadd;
            optNumericSubtract.Text = Strings.EventSetVariable.numericsubtract;
            optNumericRandom.Text = Strings.EventSetVariable.numericrandom;
            optNumericSystemTime.Text = Strings.EventSetVariable.numericsystemtime;
            optNumericClonePlayerVar.Text = Strings.EventSetVariable.numericcloneplayervariablevalue;
            optNumericCloneGlobalVar.Text = Strings.EventSetVariable.numericcloneglobalvariablevalue;
            lblNumericRandomLow.Text = Strings.EventSetVariable.numericrandomlow;
            lblNumericRandomHigh.Text = Strings.EventSetVariable.numericrandomhigh;

            //Booleanic
            grpBooleanVariable.Text = Strings.EventSetVariable.booleanlabel;
            optBooleanTrue.Text = Strings.EventSetVariable.booleantrue;
            optBooleanFalse.Text = Strings.EventSetVariable.booleanfalse;
            optBooleanCloneGlobalVar.Text = Strings.EventSetVariable.booleanccloneglobalvariablevalue;
            optBooleanClonePlayerVar.Text = Strings.EventSetVariable.booleancloneplayervariablevalue;

            //String
            grpStringVariable.Text = Strings.EventSetVariable.stringlabel;
            optStaticString.Text = Strings.EventSetVariable.stringvalue;
            optClonePlayerString.Text = Strings.EventSetVariable.stringcloneplayerstringvalue;
            optCloneGlobalString.Text = Strings.EventSetVariable.stringcloneglobalstringvalue;
            optPlayerName.Text = Strings.EventSetVariable.stringplayername;
        }

        private void InitEditor()
        {
            cmbVariable.Items.Clear();
            int varCount = 0;
            if (rdoPlayerVariable.Checked)
            {
                cmbVariable.Items.AddRange(PlayerVariableBase.Names);
                cmbVariable.SelectedIndex = PlayerVariableBase.ListIndex(mMyCommand.VariableId);
            }
            else
            {
                cmbVariable.Items.AddRange(ServerVariableBase.Names);
                cmbVariable.SelectedIndex =  ServerVariableBase.ListIndex(mMyCommand.VariableId);
            }

            

    

            chkSyncParty.Checked = mMyCommand.SyncParty;

            UpdateFormElements();
        }

        private void UpdateFormElements()
        {
            //Hide editor windows until we have a variable selected to work with
            grpNumericVariable.Hide();
            grpBooleanVariable.Hide();
            grpStringVariable.Hide();

            var varType = 0;
            if (cmbVariable.SelectedIndex > -1)
            {
                //Determine Variable Type
                if (rdoPlayerVariable.Checked)
                {
                    var playerVar = PlayerVariableBase.FromList(cmbVariable.SelectedIndex);
                    if (playerVar != null) varType = (byte)playerVar.Type;

                }
                else if (rdoGlobalVariable.Checked)
                {
                    var serverVar = ServerVariableBase.FromList(cmbVariable.SelectedIndex);
                    if (serverVar != null) varType = (byte)serverVar.Type;
                }
            }
            
            //Load the correct editor
            if (varType > 0)
            {
                switch ((VariableDataTypes) varType)
                {
                    case VariableDataTypes.Boolean:
                        grpBooleanVariable.Show();
                        TryLoadBooleanMod(mMyCommand.Modification);
                        break;

                    case VariableDataTypes.Integer:
                        grpNumericVariable.Show();
                        TryLoadNumericMod(mMyCommand.Modification);
                        UpdateNumericFormElements();
                        break;

                    case VariableDataTypes.Number:
                        break;

                    case VariableDataTypes.String:
                        grpStringVariable.Show();
                        TryLoadStringMod(mMyCommand.Modification);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

        }



        private void btnSave_Click(object sender, EventArgs e)
        {
            int n;
            if (rdoPlayerVariable.Checked)
            {
                mMyCommand.VariableType = VariableTypes.PlayerVariable;
                mMyCommand.VariableId = PlayerVariableBase.IdFromList(cmbVariable.SelectedIndex);
            }
            if (rdoGlobalVariable.Checked)
            {
                mMyCommand.VariableType = VariableTypes.ServerVariable;
                mMyCommand.VariableId = ServerVariableBase.IdFromList(cmbVariable.SelectedIndex);
            }

            if (grpNumericVariable.Visible)
            {
                mMyCommand.Modification = GetNumericVariableMod();
            }
            else if (grpBooleanVariable.Visible)
            {
                mMyCommand.Modification = GetBooleanVariableMod();
            }
            else if (grpStringVariable.Visible)
            {
                mMyCommand.Modification = GetStringVariableMod();
            }
            else
            {
                mMyCommand.Modification = new VariableMod();
            }

            mMyCommand.SyncParty = chkSyncParty.Checked;

            mEventEditor.FinishCommandEdit();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            mEventEditor.CancelCommandEdit();
        }



        private void rdoPlayerVariable_CheckedChanged(object sender, EventArgs e)
        {
            InitEditor();
            if (!mLoading && cmbVariable.Items.Count > 0) cmbVariable.SelectedIndex = 0;
            if (!mLoading) optNumericSet.Checked = true;
            if (!mLoading) nudNumericValue.Value = 0;
        }

        private void rdoGlobalVariable_CheckedChanged(object sender, EventArgs e)
        {
            InitEditor();
            if (!mLoading && cmbVariable.Items.Count > 0) cmbVariable.SelectedIndex = 0;
            if (!mLoading) optNumericSet.Checked = true;
            if (!mLoading) nudNumericValue.Value = 0;
        }

        #region "Boolean Variable"
        private void TryLoadBooleanMod(VariableMod varMod)
        {
            if (varMod == null) varMod = new BooleanVariableMod();
            if (varMod.GetType() == typeof(BooleanVariableMod))
            {
                var mod = (BooleanVariableMod)varMod;

                optBooleanTrue.Checked = mod.Value;
                optBooleanFalse.Checked = !mod.Value;

                if (mod.DuplicateVariableId != Guid.Empty)
                {
                    if (mod.DupVariableType == VariableTypes.PlayerVariable)
                    {
                        optBooleanClonePlayerVar.Checked = true;
                        cmbBooleanClonePlayerVar.SelectedIndex = PlayerVariableBase.ListIndex(mod.DuplicateVariableId);
                    }
                    else
                    {
                        optBooleanCloneGlobalVar.Checked = true;
                        cmbBooleanCloneGlobalVar.SelectedIndex = ServerVariableBase.ListIndex(mod.DuplicateVariableId);
                    }
                }
            }
        }

        private BooleanVariableMod GetBooleanVariableMod()
        {
            var mod = new BooleanVariableMod();

            mod.Value = optBooleanTrue.Checked;

            if (optBooleanClonePlayerVar.Checked)
            {
                mod.DupVariableType = VariableTypes.PlayerVariable;
                mod.DuplicateVariableId = PlayerVariableBase.IdFromList(cmbBooleanClonePlayerVar.SelectedIndex);
            }
            else if (optBooleanCloneGlobalVar.Checked)
            {
                mod.DupVariableType = VariableTypes.ServerVariable;
                mod.DuplicateVariableId = ServerVariableBase.IdFromList(cmbBooleanCloneGlobalVar.SelectedIndex);
            }

            return mod;
        }

        #endregion

        #region "Numeric Variable"

        private void TryLoadNumericMod(VariableMod varMod)
        {
            if (varMod == null) varMod = new IntegerVariableMod();
            if (varMod.GetType() == typeof(IntegerVariableMod))
            {
                var mod = (IntegerVariableMod) varMod;

                switch (mod.ModType)
                {
                    case VariableMods.Set:
                        optNumericSet.Checked = true;
                        optNumericStaticVal.Checked = true;
                        nudNumericValue.Value = mod.Value;

                        break;

                    case VariableMods.Add:
                        optNumericAdd.Checked = true;
                        optNumericStaticVal.Checked = true;
                        nudNumericValue.Value = mod.Value;

                        break;

                    case VariableMods.Subtract:
                        optNumericSubtract.Checked = true;
                        optNumericStaticVal.Checked = true;
                        nudNumericValue.Value = mod.Value;

                        break;

                    case VariableMods.Random:
                        optNumericRandom.Checked = true;
                        nudLow.Value = mod.Value;
                        nudHigh.Value = mod.HighValue;

                        break;

                    case VariableMods.SystemTime:
                        optNumericSystemTime.Checked = true;

                        break;

                    case VariableMods.DupPlayerVar:
                        optNumericSet.Checked = true;
                        optNumericClonePlayerVar.Checked = true;
                        cmbNumericClonePlayerVar.SelectedIndex = PlayerVariableBase.ListIndex(mod.DuplicateVariableId);

                        break;

                    case VariableMods.DupGlobalVar:
                        optNumericSet.Checked = true;
                        optNumericCloneGlobalVar.Checked = true;
                        cmbNumericCloneGlobalVar.SelectedIndex = ServerVariableBase.ListIndex(mod.DuplicateVariableId);

                        break;

                    case VariableMods.AddPlayerVar:
                        optNumericAdd.Checked = true;
                        optNumericClonePlayerVar.Checked = true;
                        cmbNumericClonePlayerVar.SelectedIndex = PlayerVariableBase.ListIndex(mod.DuplicateVariableId);

                        break;

                    case VariableMods.AddGlobalVar:
                        optNumericAdd.Checked = true;
                        optNumericCloneGlobalVar.Checked = true;
                        cmbNumericCloneGlobalVar.SelectedIndex = ServerVariableBase.ListIndex(mod.DuplicateVariableId);

                        break;

                    case VariableMods.SubtractPlayerVar:
                        optNumericSubtract.Checked = true;
                        optNumericClonePlayerVar.Checked = true;
                        cmbNumericClonePlayerVar.SelectedIndex = PlayerVariableBase.ListIndex(mod.DuplicateVariableId);

                        break;

                    case VariableMods.SubtractGlobalVar:
                        optNumericSubtract.Checked = true;
                        optNumericCloneGlobalVar.Checked = true;
                        cmbNumericCloneGlobalVar.SelectedIndex = ServerVariableBase.ListIndex(mod.DuplicateVariableId);

                        break;
                }
            }
        }

        private void UpdateNumericFormElements()
        {
            grpNumericRandom.Visible = optNumericRandom.Checked;
            grpNumericValues.Visible = optNumericAdd.Checked | optNumericSubtract.Checked | optNumericSet.Checked;
        }

        private void optNumericSet_CheckedChanged(object sender, EventArgs e)
        {
            UpdateNumericFormElements();
        }

        private void optNumericAdd_CheckedChanged(object sender, EventArgs e)
        {
            UpdateNumericFormElements();
        }

        private void optNumericSubtract_CheckedChanged(object sender, EventArgs e)
        {
            UpdateNumericFormElements();
        }

        private void optNumericRandom_CheckedChanged(object sender, EventArgs e)
        {
            UpdateNumericFormElements();
        }

        private void optNumericSystemTime_CheckedChanged(object sender, EventArgs e)
        {
            UpdateNumericFormElements();
        }

        private void optNumericClonePlayerVar_CheckedChanged(object sender, EventArgs e)
        {
            UpdateNumericFormElements();
        }

        private void optNumericCloneGlobalVar_CheckedChanged(object sender, EventArgs e)
        {
            UpdateNumericFormElements();
        }

        private IntegerVariableMod GetNumericVariableMod()
        {
            var mod = new IntegerVariableMod();
            if (optNumericSet.Checked && optNumericStaticVal.Checked)
            {
                mod.ModType = VariableMods.Set;
                mod.Value = (int)nudNumericValue.Value;
            }
            else if (optNumericAdd.Checked && optNumericStaticVal.Checked)
            {
                mod.ModType = VariableMods.Add;
                mod.Value = (int)nudNumericValue.Value;
            }
            else if (optNumericSubtract.Checked && optNumericStaticVal.Checked)
            {
                mod.ModType = VariableMods.Subtract;
                mod.Value = (int)nudNumericValue.Value;
            }
            else if (optNumericRandom.Checked)
            {
                mod.ModType = VariableMods.Random;
                mod.Value = (int)nudLow.Value;
                mod.HighValue = (int)nudHigh.Value;
                if (mod.HighValue < mod.Value)
                {
                    var n = mod.Value;
                    mod.Value = mod.HighValue;
                    mod.HighValue = n;
                }
            }
            else if (optNumericSystemTime.Checked)
            {
                mod.ModType = VariableMods.SystemTime;
            }
            else if (optNumericClonePlayerVar.Checked)
            {
                if (optNumericSet.Checked)
                {
                    mod.ModType = VariableMods.DupPlayerVar;
                }
                else if (optNumericAdd.Checked)
                {
                    mod.ModType = VariableMods.AddPlayerVar;
                }
                else
                {
                    mod.ModType = VariableMods.SubtractPlayerVar;
                }
                mod.DuplicateVariableId = PlayerVariableBase.IdFromList(cmbNumericClonePlayerVar.SelectedIndex);
            }
            else if (optNumericCloneGlobalVar.Checked)
            {
                if (optNumericSet.Checked)
                {
                    mod.ModType = VariableMods.DupGlobalVar;
                }
                else if (optNumericAdd.Checked)
                {
                    mod.ModType = VariableMods.AddGlobalVar;
                }
                else
                {
                    mod.ModType = VariableMods.SubtractGlobalVar;
                }
                mod.DuplicateVariableId = ServerVariableBase.IdFromList(cmbNumericCloneGlobalVar.SelectedIndex);
            }

            return mod;
        }

        #endregion

        #region "String Variable"

        private void TryLoadStringMod(VariableMod varMod)
        {
            if (varMod == null) varMod = new StringVariableMod();
            if (varMod.GetType() == typeof(StringVariableMod))
            {
                var mod = (StringVariableMod)varMod;

                switch (mod.ModType)
                {
                    case VariableMods.Set:
                        optStaticString.Checked = true;
                        txtStringValue.Text = mod.Value;
                        break;
                    case VariableMods.DupPlayerVar:
                        optClonePlayerString.Checked = true;
                        cmbDupPlayerString.SelectedIndex = PlayerVariableBase.ListIndex(mod.DuplicateVariableId);
                        break;
                    case VariableMods.DupGlobalVar:
                        optCloneGlobalString.Checked = true;
                        cmbDupGlobalString.SelectedIndex = ServerVariableBase.ListIndex(mod.DuplicateVariableId);
                        break;
                    case VariableMods.PlayerName:
                        optPlayerName.Checked = true;
                        break;
                }
            }
        }

        private StringVariableMod GetStringVariableMod()
        {
            var mod = new StringVariableMod();
            if (optStaticString.Checked)
            {
                mod.ModType = VariableMods.Set;
                mod.Value = txtStringValue.Text;
            }
            else if (optClonePlayerString.Checked)
            {
                mod.ModType = VariableMods.DupPlayerVar;
                mod.DuplicateVariableId = PlayerVariableBase.IdFromList(cmbDupPlayerString.SelectedIndex);
            }
            else if (optCloneGlobalString.Checked)
            {
                mod.ModType = VariableMods.DupGlobalVar;
                mod.DuplicateVariableId = ServerVariableBase.IdFromList(cmbDupGlobalString.SelectedIndex);
            }
            else if (optPlayerName.Checked)
            {
                mod.ModType = VariableMods.PlayerName;
            }

            return mod;
        }

        #endregion

        private void cmbVariable_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFormElements();
        }
    }
}