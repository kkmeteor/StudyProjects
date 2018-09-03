using System;
using System.Collections.Generic;
using System.Text;
using Infragistics.UltraChart.Design;

namespace UFIDA.U8.UAP.Services.ReportElements
{
    public class ChartWizardAdapter
    {
        private ChartWizard _chartwizard;
        private ChartService _chartservice;
        public event DeletingSchemaHandler DeletedASchema;
        public ChartWizardAdapter(ChartService chartservice)
        {
            _chartservice = chartservice;
            _chartwizard = new ChartWizard();
            _chartwizard.SchemaChanging += new SchemaChangingHandler(_chartwizard_SchemaChanging);
            _chartwizard.SchemaCompleted += new SchemaCompletedHander(_chartwizard_SchemaCompleted);
            _chartwizard.SchemaCaptionChanged += new SchemaCaptionChangedHandler(_chartwizard_SchemaCaptionChanged);
            _chartwizard.AddingSchema += new AddingSchamaHandler(_chartwizard_AddingSchema);
            _chartwizard.DeletingSchema += new DeletingSchemaHandler(_chartwizard_DeletingSchema);
            _chartwizard.DefaultSchema += new DefaultSchemaHandler(_chartwizard_DefaultSchema);
            _chartwizard.CopySchema += new CopySchamaHandler(_chartwizard_CopySchema);
        }

        private SchemaIdentity _chartwizard_CopySchema(int level, string id)
        {
            ChartSchemaItemAmong among = _chartservice.AddASchema(level,id);
            return new SchemaIdentity(among.ID, among.Caption);
        }

        public void UpdateChartService(ChartService chartservice)
        {
            _chartservice = chartservice;
        }

        private void _chartwizard_DefaultSchema(int level, string id)
        {
            _chartservice.SetDefaultID(level, id);
        }

        public ChartWizard ChartWizard
        {
            get
            {
                return _chartwizard;
            }
        }

        private void _chartwizard_DeletingSchema(int level, string id)
        {
            _chartservice.DeleteChart(level, id);
            if (DeletedASchema != null)
                DeletedASchema(level, id);
        }

        private SchemaIdentity _chartwizard_AddingSchema(int level)
        {
            ChartSchemaItemAmong among = _chartservice.AddASchema(level);
            return new SchemaIdentity(among.ID, among.Caption);
        }

        private void _chartwizard_SchemaCaptionChanged(int level, string id, string caption)
        {
            ChartSchemaItemAmong among = _chartservice.GetSchemaAmongBy(level, id);
            among.Caption = caption;
        }

        private void _chartwizard_SchemaCompleted(int level, string id)
        {
            ChartSchemaItemAmong among = _chartservice.GetSchemaAmongBy(level, id);
            _chartservice.SaveSettings(_chartwizard.WizardPane, among, level);
        }

        private void _chartwizard_SchemaChanging(int level, string id)
        {
            _chartservice.InitializeChartWizard(level, id, _chartwizard.WizardPane);
        }
    }
}
