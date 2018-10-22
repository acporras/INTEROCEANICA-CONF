﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using FinalXML.Administradores;
using FinalXML.Entidades;
using System.IO;
using System.Net.Http;
using FinalXML;
using FinalXML.Properties;
using FinalXML.Informes;
using System.Text.RegularExpressions;
using Tesseract;
using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Imaging.Textures;
using System.Configuration;
using System.Net;
using System.Xml;
using FinalXML.BD;

namespace FinalXML
{
    public partial class Form2 : MetroFramework.Forms.MetroForm /*PlantillaBase*/
    {
        private DocumentoElectronico _documento;
        private ResumenDiario _resumen;
        Herramientas herramientas = new Herramientas();
        clsCargaVentas CVentas = new clsCargaVentas();
        clsCargaVentas CVentas1 = new clsCargaVentas();
        clsPedido Pedido = new clsPedido();
        clsAdmCargaVentas AdmCVenta = new clsAdmCargaVentas();
        clsAdmEmpresa AdmCEmpresa = new clsAdmEmpresa();
        clsAdmPedido AdmPedido = new clsAdmPedido();
        Conversion ConvertLetras = new Conversion();
        public static BindingSource data = new BindingSource();
        String filtro = String.Empty;
        public string TramaXmlSinFirma { get; set; }
        public string RutaArchivo { get; set; }
        public string IdDocumento { get; set; }
        public String recursos;
        public DataTable dt_Ventas = new DataTable();
        public DataTable dt_Empresa = new DataTable();
        public DataTable dt_DetalleVenta = new DataTable();
        public DataTable dt_Pedidos = new DataTable();
        public DataTable dt_DetallePedido = new DataTable();
        public Int32 Proceso = 0;
        public String CodTipoDocumento; //Utilizado para el tipo de documento anulacion
        public String SunatFact = ConfigurationManager.AppSettings.Get("SUNATCPE");
        public String SunatGuia = ConfigurationManager.AppSettings.Get("SUNATGUI");
        public String SunatOtro = ConfigurationManager.AppSettings.Get("SUNATOCE");

        #region Métodos
        public Form2()
        {
            InitializeComponent();
            _documento = new DocumentoElectronico
            {
                FechaEmision = DateTime.Today.ToShortDateString(),
                //Emisor=CrearEmisor()
                //IdDocumento = Numera.Serie+ "-" + str.PadLeft(8, pad)
            };
            _resumen = new ResumenDiario();
            recursos = herramientas.GetResourcesPath();
            grvResDetail.Rows.Clear();
            CargaEmpresa();
            cboEstadoEmisor.SelectedIndex = 0;
            cboTipdoc.SelectedIndex = 0;
        }
        private void CargaEmpresa()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Int32 index = 0;
                dt_Empresa = AdmCEmpresa.CargaEmpresa();
                cboEmpresa.DataSource = dt_Empresa;
                cboEmpresa.ValueMember = "NU_EMINUMRUC";
                cboEmpresa.DisplayMember = "NO_EMIRAZSOC";

                cboEmpresaDoc.DataSource = dt_Empresa;
                cboEmpresaDoc.ValueMember = "NU_EMINUMRUC";
                cboEmpresaDoc.DisplayMember = "NO_EMIRAZSOC";

                cboEmpresaBaj.DataSource = dt_Empresa;
                cboEmpresaBaj.ValueMember = "NU_EMINUMRUC";
                cboEmpresaBaj.DisplayMember = "NO_EMIRAZSOC";
                grvEmisores.Rows.Clear();
                grvEmisores.ClearSelection();
                foreach (DataRow row in dt_Empresa.Rows) {
                    grvEmisores.Rows.Add(row["NU_EMINUMRUC"].ToString(), row["NO_EMIRAZSOC"].ToString(), row["CO_EMICODAGE"].ToString(),
                        row["NO_ESTEMIELE"].ToString(), row["NO_CONEMIELE"].ToString(), row["NO_EMIUBIGEO"].ToString(),
                        row["NO_EMIDEPART"].ToString(), row["NO_EMIPROVIN"].ToString(), row["NO_EMIDISTRI"].ToString(),
                        row["NO_EMIDIRFIS"].ToString(), row["NO_BASNOMSRV"].ToString(), row["NO_BASNOMBAS"].ToString(),
                        row["NO_TABFACCAB"].ToString(), row["NO_TABFACDET"].ToString(), (row["FL_REGINACTI"].ToString() == "0") ? "Activo" : "Inactivo");
                }
            }
            catch (Exception a) { MessageBox.Show(a.Message); }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private Contribuyente LeerEmpresa(String NumRuc)
        {
            return AdmCEmpresa.LeerEmpresa(NumRuc);
        }
        private void CargaVentas() {
            Cursor.Current = Cursors.WaitCursor;
            try {
                /*dgListadoVentas.DataSource = data;
                data.DataSource = AdmCVenta.CargaVentas(dtpDesde.Value,dtpHasta.Value);
                data.Filter = String.Empty;
                filtro = String.Empty;*/
                

                Int32 index = 0;
                String TipDoc = "";
                switch (cboTipdoc.SelectedIndex) {
                    case 1:
                        TipDoc = "FT";
                        break;
                    case 2:
                        TipDoc = "BV";
                        break;
                    case 3:
                        TipDoc = "NC";
                        break;
                    case 4:
                        TipDoc = "ND";
                        break;
                    default:
                        TipDoc = "";
                        break;
                }
                //dt_Ventas = AdmCVenta.CargaDocumentos(cboEmpresaDoc.SelectedValue.ToString() ,dtpDesde.Value.Date, dtpHasta.Value.Date, "");
                dt_Ventas = AdmCVenta.CargaDocumentos(cboEmpresaDoc.SelectedValue.ToString(), dtpDesde.Value.Date, dtpHasta.Value.Date, TipDoc);
                dgListadoVentas.Rows.Clear();
                dgListadoVentas.ClearSelection();
                foreach (DataRow row in dt_Ventas.Rows)
                {
                    dgListadoVentas.Rows.Add(row[0], row[1], row[2], row[3], row[4], row[5], row[6], row[7],
                        row[8], row[9], row[10], row[11],"","","",row[12], row[13], row[14]);
                    if (row[11].ToString() == "ACEPTADA") {
                        dgListadoVentas.Rows[index].DefaultCellStyle.BackColor = Color.Aquamarine;
                    } else if (row[11].ToString() == "RECHAZADO") {
                        dgListadoVentas.Rows[index].DefaultCellStyle.BackColor = Color.Red;
                    } else if (row[11].ToString()== "POR ENVIAR") {
                        dgListadoVentas.Rows[index].DefaultCellStyle.BackColor = Color.Cornsilk;
                    }else if (row[11].ToString() == "ANULADO")
                    {
                        dgListadoVentas.Rows[index].DefaultCellStyle.BackColor = Color.Pink;
                    }

                    index++;
                }
                Proceso = 0;
            }
            catch (Exception a) { MessageBox.Show(a.Message); }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private void CargaBoletas()
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                Int32 index = 0;
                dt_Ventas = AdmCVenta.CargaDocumentos(cboEmpresa.SelectedValue.ToString(), dtpFecIni.Value.Date, dtpFecFin.Value.Date, "BV", 0);
                grvResDetail.Rows.Clear();
                grvResDetail.ClearSelection();
                foreach (DataRow row in dt_Ventas.Rows)
                {
                    grvResDetail.Rows.Add(row[3], row[4], row[5], row[5], row[7], row[8]);
                    if (row[11].ToString() == "ACEPTADA")
                    {
                        grvResDetail.Rows[index].DefaultCellStyle.BackColor = Color.Aquamarine;
                    }
                    else if (row[11].ToString() == "RECHAZADO")
                    {
                        grvResDetail.Rows[index].DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (row[11].ToString() == "POR ENVIAR")
                    {
                        grvResDetail.Rows[index].DefaultCellStyle.BackColor = Color.Cornsilk;
                    }
                    else if (row[11].ToString() == "ANULADO")
                    {
                        grvResDetail.Rows[index].DefaultCellStyle.BackColor = Color.Pink;
                    }

                    index++;
                }
                Proceso = 0;
            }
            catch (Exception a) { MessageBox.Show(a.Message); }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
        private bool ValidaRuc(Int64 ruc)
        {
            if (!(ruc >= 1e10 && ruc < 11e9 || ruc >= 15e9 && ruc < 18e9 || ruc >= 2e10 && ruc < 21e9)) {
                return false;
            }
            Int64 suma = (ruc % 10 < 2) ? -1 : -0;
            for (int i = 0; i < 11; i++, ruc = ruc / 10 | 0)
            {
                suma += (ruc % 10) * (i % 7 + (i / 7 | 0) + 1);
            }
            return (suma % 11 == 0) ? true : false;
        }
        private void EnviarResumen()
        {
            //Se valida que existan datos en la grilla
            var contribuyente = LeerEmpresa(cboEmpresa.SelectedValue.ToString());
            if (grvResDetail.RowCount - 1 > 0)
            {
                int Serie = AdmCEmpresa.GetCorrelativoMasivo(contribuyente.CodigoEmpresa, "RC");
                //Datos del Resumen
                DateTime FechaActual = DateTime.Today;
                _resumen.IdDocumento = String.Format(@"RC-{0}{1}{2}-{3}", FechaActual.Year, String.Format("{0:00}", FechaActual.Month), String.Format("{0:00}", FechaActual.Day), Serie);
                _resumen.Emisor = contribuyente;
                _resumen.FechaEmision = FechaActual.ToString("yyyy-MM-dd");
                _resumen.FechaReferencia = FechaActual.ToString("yyyy-MM-dd");

                //Cabecera
                String tipdoc = "", sersun = "", numsun = "";
                Decimal impuesto = 0, Gravadas = 0, Exoneradas = 0;
                //ven.SubTotalVenta = Math.Abs((Convert.ToDecimal(row[7]) - Convert.ToDecimal(row[6])));
                List<GrupoResumen> Items = new List<GrupoResumen>();
                GrupoResumen ven = new GrupoResumen();
                int i = 0;
                foreach (DataGridViewRow row in grvResDetail.Rows)
                {
                    i++;
                    tipdoc = row.Cells["numdoc"].Value.ToString().Substring(0, 2);
                    sersun = row.Cells["numdoc"].Value.ToString().Substring(2, 4);
                    numsun = row.Cells["numdoc"].Value.ToString().Substring(6, 7);
                    CVentas1 = AdmCVenta.LeerVenta(_resumen.Emisor.NroDocumento, tipdoc, sersun, numsun);

                    dt_DetalleVenta = AdmCVenta.LeerDetalle(cboEmpresa.SelectedValue.ToString(), CVentas1.Sigla, CVentas1.Serie, CVentas1.Numeracion);
                    if (dt_DetalleVenta != null)
                    {
                        foreach (DataRow row_det in dt_DetalleVenta.Rows)
                        {
                            impuesto += Convert.ToDecimal(row_det[6].ToString());
                            if (Math.Abs((Convert.ToDecimal(row_det[6]))) != 0)
                            {
                                Gravadas += (CVentas1.Moneda == "MN") ? Math.Abs((Convert.ToDecimal(row_det[7]) - Convert.ToDecimal(row_det[6]))) :
                                Math.Abs((Convert.ToDecimal(row_det[9]) - Convert.ToDecimal(row_det[6])));
                            }
                            else
                            {
                                Exoneradas += (CVentas1.Moneda == "MN") ? Math.Abs((Convert.ToDecimal(row_det[7]) - Convert.ToDecimal(row_det[6]))) :
                                Math.Abs((Convert.ToDecimal(row_det[9]) - Convert.ToDecimal(row_det[6])));
                            }
                        }
                    }
                    ven = new GrupoResumen
                    {
                        Id = i,
                        TipoDocumento = "03",
                        Moneda = "PEN",
                        NumeroDocumento = sersun + "-" + numsun,
                        TotalVenta = Gravadas + Exoneradas + impuesto,
                        TotalDescuentos = 0,
                        TotalIgv = impuesto,
                        TotalIsc = 0,
                        TotalOtrosImpuestos = 0,
                        Gravadas = Gravadas,
                        Exoneradas = Exoneradas,
                        Inafectas = 0,
                        Exportacion = 0,
                        Gratuitas = 0,
                        DocumentoCliente = CVentas1.NumDocCliente,
                        TipoDocumentoCliente = "1",
                        Operacion = "1"
                    };
                    Items.Add(ven);
                }

                _resumen.Resumenes = Items;

                var ResumenDiario = GeneradorXML.GenerarSummaryDocuments(_resumen);
                var serializador3 = new Serializador();
                TramaXmlSinFirma = serializador3.GenerarXml(ResumenDiario);

                RutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documentos\\" +
                    $"{_resumen.IdDocumento}.xml");
                File.WriteAllBytes(RutaArchivo, Convert.FromBase64String(TramaXmlSinFirma));

                IdDocumento = _resumen.IdDocumento;

                var firmadoRequest = new FirmadoRequest
                {
                    TramaXmlSinFirma = TramaXmlSinFirma,
                    CertificadoDigital = Convert.ToBase64String(File.ReadAllBytes(recursos + "\\INTEROCEANICAPFX.pfx")),
                    PasswordCertificado = "uY9eYH8utq4SyreY", //546IUYJHGT5
                    UnSoloNodoExtension = true //rbRetenciones.Checked || rbResumen.Checked

                };

                FirmarController enviar = new FirmarController();

                var respuestaFirmado = enviar.FirmadoResponse(firmadoRequest);
                var oContribuyente = LeerEmpresa(cboEmpresa.SelectedValue.ToString());
                var enviarDocumentoRequest = new EnviarDocumentoRequest
                {
                    Ruc = cboEmpresa.SelectedValue.ToString(),
                    //UsuarioSol = "FACTURA1",
                    //ClaveSol = "FACTURA1",
                    UsuarioSol = oContribuyente.UsuarioSol,
                    ClaveSol = oContribuyente.ClaveSol,
                    //EndPointUrl = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService",
                    EndPointUrl = SunatFact,
                    IdDocumento = _resumen.IdDocumento,
                    TipoDocumento = "RC",
                    TramaXmlFirmado = respuestaFirmado.TramaXmlFirmado
                };
                var respuestaEnvio = new EnviarDocumentoResponse();
                EnviarResumenController enviaResumen = new EnviarResumenController();
                respuestaEnvio = enviaResumen.EnviarResumenResponse(enviarDocumentoRequest);


                var rpta = (EnviarDocumentoResponse)respuestaEnvio;
                txtDetailRes.Text = $@"{Resources.procesoCorrecto}{Environment.NewLine}{rpta.NroTicket}";
                if (rpta.Exito)
                {
                    //Se actualiza el estado de las boletas enviadas a 'ACEPTADA'
                    foreach (DataGridViewRow row in grvResDetail.Rows)
                    {
                        CVentas1.Sigla = row.Cells["numdoc"].Value.ToString().Substring(0, 2);
                        CVentas1.Serie = row.Cells["numdoc"].Value.ToString().Substring(2, 4);
                        CVentas1.Numeracion = row.Cells["numdoc"].Value.ToString().Substring(6, 7);
                        CVentas1.NumDocEmisor = oContribuyente.NroDocumento;
                        CVentas1.CodigoRespuesta = "";
                        CVentas1.MensajeRespuesta = rpta.NroTicket.ToString();
                        CVentas1.NombreArchivo = rpta.NombreArchivo + ".xml";
                        CVentas1.NombreArchivoCDR = "R-" + rpta.NombreArchivo + ".zip";
                        CVentas1.NombreArchivoPDF = oContribuyente.NroDocumento + "-" + DateTime.Parse(row.Cells["fecemi"].Value.ToString()).ToString("yyyy-MM-dd") + "-" + row.Cells["numdoc"].Value.ToString() + ".pdf";
                        if (CVentas1 != null && CVentas1.Numeracion != "")
                        {
                            CVentas1.EstadoDocSunat = 0;
                            AdmCVenta.update(CVentas1);
                        }
                    }
                    //
                    txtNumTicketResumen.Text = rpta.NroTicket.ToString();
                    var newocor = AdmCEmpresa.SetCorrelativoMasivo(contribuyente.CodigoEmpresa, "RC", Serie);
                }
                if (!respuestaEnvio.Exito)
                    throw new ApplicationException(respuestaEnvio.MensajeError);

                MessageBox.Show("Se ha enviado correctamente el archivo de resumen..!");
            }
            else
            {
                MessageBox.Show("No se han encontrado boletas para añadir al resumen..!");
            }

        }
        private bool GuardarEmisor() {
            DataRowView drv_base = (DataRowView)cboBaseDatos.SelectedItem;
            DataRowView drv_tabcab = (DataRowView)cboTablaCab.SelectedItem;
            DataRowView drv_tabdet = (DataRowView)cboTablaDet.SelectedItem;
            clsEmpresa empresa = new clsEmpresa
            {
                nu_eminumruc = txtnumruc.Text,
                no_emirazsoc = txtrazsoc.Text,
                co_emicodage = txtCodAge.Text,
                no_estemiele = txtestemi.Text,
                no_conemiele = txtconemi.Text,
                no_emiubigeo = txtubigeo.Text,
                no_emidepart = txtnomdep.Text,
                no_emiprovin = txtnomprv.Text,
                no_emidistri = txtnomdis.Text,
                no_emidirfis = txtdomfis.Text,
                no_bastipbas = "SQL",
                no_basnomsrv = txtserver.Text,
                no_basnombas = drv_base[0].ToString(),
                no_basusrbas = txtuser.Text,
                no_basusrpas = txtpass.Text,
                no_tabfaccab = drv_tabcab[1].ToString(),
                no_tabfacdet = drv_tabdet[1].ToString(),
                no_ususolsun = txtusersun.Text,
                no_passolsun = txtpasssun.Text,
                fl_reginacti = cboEstadoEmisor.SelectedIndex.ToString()
            };
            return AdmCEmpresa.GuardarEmpresa(empresa);
        }
        public bool ActualizarEmisor()
        {
            DataRowView drv_base = (DataRowView)cboBaseDatos.SelectedItem;
            DataRowView drv_tabcab = (DataRowView)cboTablaCab.SelectedItem;
            DataRowView drv_tabdet = (DataRowView)cboTablaDet.SelectedItem;
            clsEmpresa empresa = new clsEmpresa
            {
                nu_eminumruc = txtnumruc.Text,
                no_emirazsoc = txtrazsoc.Text,
                co_emicodage = txtCodAge.Text,
                no_estemiele = txtestemi.Text,
                no_conemiele = txtconemi.Text,
                no_emiubigeo = txtubigeo.Text,
                no_emidepart = txtnomdep.Text,
                no_emiprovin = txtnomprv.Text,
                no_emidistri = txtnomdis.Text,
                no_emidirfis = txtdomfis.Text,
                no_bastipbas = "SQL",
                no_basnomsrv = txtserver.Text,
                no_basnombas = drv_base[0].ToString(),
                no_basusrbas = txtuser.Text,
                no_basusrpas = txtpass.Text,
                no_tabfaccab = drv_tabcab[1].ToString(),
                no_tabfacdet = drv_tabdet[1].ToString(),
                no_ususolsun = txtusersun.Text,
                no_passolsun = txtpasssun.Text,
                fl_reginacti = cboEstadoEmisor.SelectedIndex.ToString()
            };
            return AdmCEmpresa.ActualizarEmpresa(empresa);
        }
        private void loadTables()
        {
            DataRowView drv_base = (DataRowView)cboBaseDatos.SelectedItem;
            if (drv_base[0].ToString() != "")
            {
                BaseDatos bdemi = new BaseDatos(txtserver.Text, BaseDatos.BBDD.SQL, drv_base[0].ToString(), txtuser.Text, txtpass.Text);
                bdemi.Conectar();
                DataTable dt_tablescab = new DataTable();
                DataTable dt_tablesdet = new DataTable();
                bdemi.Dame_Datos_DT("SELECT ID,NAME FROM SYSOBJECTS WHERE TYPE='U' ORDER BY NAME", false, ref dt_tablescab, "S");
                bdemi.Dame_Datos_DT("SELECT ID,NAME FROM SYSOBJECTS WHERE TYPE='U' ORDER BY NAME", false, ref dt_tablesdet, "S");

                bdemi.Desconectar();
                cboTablaCab.DataSource = dt_tablescab;
                cboTablaCab.ValueMember = "ID";
                cboTablaCab.DisplayMember = "NAME";

                cboTablaDet.DataSource = dt_tablesdet;
                cboTablaDet.ValueMember = "ID";
                cboTablaDet.DisplayMember = "NAME";
            }
        }
        /*private static Contribuyente CrearEmisor()
        {
            return new Contribuyente
            {
                NroDocumento = "20513258934",
                TipoDocumento = "6",
                Direccion = "JR. ARNALDO ALVARADO DEGREGORI 227 NRO. 227 DPTO. 301 URB. MONTERRICO CHICO LIMA - LIMA - SANTIAGO DE SURCO",
                Departamento = "LIMA",
                Provincia = "LIMA",
                Distrito = "SANTIAGO DE SURCO",
                NombreLegal = "TRANSPORTES INTEROCEANICA S.A.C.",
                NombreComercial = "",
                Ubigeo = "150140"

            };
        }*/

        private void CalcularTotales()
        {
            // Realizamos los cálculos respectivos.
            _documento.TotalIgv = _documento.Items.Sum(d => d.Impuesto);
            _documento.TotalIsc = _documento.Items.Sum(d => d.ImpuestoSelectivo);
            _documento.TotalOtrosTributos = _documento.Items.Sum(d => d.OtroImpuesto);

            _documento.Gravadas = _documento.Items
                 .Where(d => d.TipoImpuesto.StartsWith("1"))
                 .Sum(d => d.SubTotalVenta);
            //_documento.Gravadas = _documento.SubTotalVenta;

            _documento.Exoneradas = _documento.Items
                .Where(d => d.TipoImpuesto.Contains("20"))
                .Sum(d => d.Suma);

            _documento.Inafectas = _documento.Items
                .Where(d => d.TipoImpuesto.StartsWith("3") || d.TipoImpuesto.Contains("40"))
                .Sum(d => d.Suma);

            _documento.Gratuitas = _documento.Items
                .Where(d => d.TipoImpuesto.Contains("21"))
                .Sum(d => d.Suma);

            // Cuando existe ISC se debe recalcular el IGV.
            if (_documento.TotalIsc > 0)
            {
                _documento.TotalIgv = (_documento.Gravadas + _documento.TotalIsc) * _documento.CalculoIgv;
                // Se recalcula nuevamente el Total de Venta.
            }

            _documento.TotalVenta = _documento.Gravadas + _documento.Exoneradas + _documento.Inafectas +
                                     _documento.TotalIgv + _documento.TotalIsc + _documento.TotalOtrosTributos;
             _documento.MontoEnLetras = ConvertLetras.enletras(_documento.TotalVenta.ToString());


            //montoEnLetrasTextBox.Text = _documento.MontoEnLetras;
            if (_documento.CalculoIgv > 0)
            {
                _documento.SubTotalVenta = _documento.TotalVenta - _documento.TotalIgv;
            }
            else
            {
                _documento.SubTotalVenta = _documento.TotalVenta;
            }
            //documentoElectronicoBindingSource.ResetBindings(false);

        }

        private void GeneraPDF() {
            try
            {
                string codigoTipoDoc = "";
                switch (_documento.TipoDocumento)
                {
                    case "01":
                        codigoTipoDoc = "01";
                        break;
                    case "03":
                        codigoTipoDoc = "03";
                        break;
                    case "07":
                        codigoTipoDoc = "07";
                        break;
                    case "08":
                        codigoTipoDoc = "08";
                        break;

                    case "20":
                        codigoTipoDoc = "20";
                        break;
                }
                if (codigoTipoDoc == "")
                {
                    MessageBox.Show("Seleccione Tipo de Documento");
                    return;
                }

                if (_documento.Items.Count < 1 && _documento.Receptor.NroDocumento == "") {
                    MessageBox.Show("No se puede generar PDF");
                    return;
                }

                if (codigoTipoDoc == "01")
                {

                    if (_documento.Receptor.TipoDocumento == "6")
                    {

                        FrmFactura2 form = new FrmFactura2("Informes\\DTFactura2.rdlc", _documento);
                        form._documento = _documento;
                        form.ShowDialog();

                    }



                }
                else
                {
                    if (codigoTipoDoc == "03")
                    {
                        FrmBoletas form = new FrmBoletas("Informes\\DTBoletas.rdlc", _documento);
                        form._documento = _documento;
                        form.ShowDialog();

                    }
                    else
                    {

                        if (codigoTipoDoc == "07")//NC
                        {

                            FrmNC form = new FrmNC("Informes\\DTNC.rdlc", _documento);
                            form._documento = _documento;
                            form.ShowDialog();
                        }
                        else
                        {
                            if (codigoTipoDoc == "08")//ND
                            {

                                FrmND form = new FrmND("Informes\\DTND.rdlc", _documento);
                                form._documento = _documento;
                                form.ShowDialog();
                            }
                            else
                            {
                                if (codigoTipoDoc == "20") //Retención
                                {
                                    FrmRetencion form = new FrmRetencion("Informes\\DTRetencion.rdlc", _documento);
                                    form._documento = _documento;
                                    form.ShowDialog();
                                }
                            }
                        }
                    }

                }
            }
            catch (Exception a) { MessageBox.Show(a.Message); }

        }
        private bool AccesoInternet()
        {

            try
            {
                System.Net.IPHostEntry host = System.Net.Dns.GetHostEntry("www.google.com");
                return true;

            }
            catch (Exception es)
            {

                return false;
            }

        }
        #endregion Fin Métodos
        
        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                CargaVentas();
                //CargaPedidos();


            }
            catch (Exception a ) { MessageBox.Show(a.Message); }
        }

        private void btnGeneraXML_Click(object sender, EventArgs e)
        {
            try
            {
                _documento = new DocumentoElectronico
                {
                    //FechaEmision = DateTime.Today.ToShortDateString(),
                    //Emisor = CrearEmisor()
                    Emisor = LeerEmpresa(cboEmpresaDoc.SelectedValue.ToString())
                //IdDocumento = Numera.Serie+ "-" + str.PadLeft(8, pad)
                };
                List<DetalleDocumento> Items = new List<DetalleDocumento>();
                DetalleDocumento ven = null;
                Cursor.Current = Cursors.WaitCursor;
                //Bucar Datos del Documento seleccionado
                if (dgListadoVentas.RowCount >= 1 && dgListadoVentas.SelectedRows.Count >= 1)
                {
                    //Cabecera
                    CVentas1 = AdmCVenta.LeerVenta(CVentas.Sigla, CVentas.Serie, CVentas.Numeracion);
                    //Detalle
                    if (CVentas1.Serie != null && CVentas1.Sigla != null && CVentas1.Numeracion != null)
                    {
                        if (CVentas1.Moneda == "MN")
                        {

                            _documento.Moneda = "PEN";
                        }
                        else if (CVentas1.Moneda == "US") {
                            _documento.Moneda = "USD";
                        }
                       
                        dt_DetalleVenta = AdmCVenta.LeerDetalle(cboEmpresaDoc.SelectedValue.ToString() ,CVentas.Sigla, CVentas.Serie, CVentas.Numeracion);
                        if (dt_DetalleVenta != null) {

                            int i = 0;

                            foreach (DataRow row in dt_DetalleVenta.Rows) {

                                if (Convert.ToString(row[1]).Trim() != "TXT")
                                {
                                    if (i > 0) Items.Add(ven);
                                    ven = new DetalleDocumento();
                                    ven.Id = Convert.ToInt32(row[0]);
                                    ven.CodigoItem =Convert.ToString(row[1]);
                                    ven.Descripcion =Convert.ToString(row[2]).Trim();                                
                                    ven.Cantidad =Math.Abs(Convert.ToDecimal(row[4]));
                                    ven.PrecioUnitario = Math.Abs(Convert.ToDecimal(row[5]));
                                    if (_documento.Moneda == "PEN") {
                                        ven.Suma = Math.Abs((Convert.ToDecimal(row[7])));
                                        ven.SubTotalVenta = Math.Abs((Convert.ToDecimal(row[7]) - Convert.ToDecimal(row[6])));
                                    }
                                    else if(_documento.Moneda=="USD") {
                                        ven.Suma = Math.Abs(Convert.ToDecimal(row[9]));//Math.Round(ven.PrecioUnitario * ven.Cantidad, 2);
                                        ven.SubTotalVenta = Math.Abs((Convert.ToDecimal(row[9]) - Convert.ToDecimal(row[6])));
                                    }
                                                                       
                                    ven.Impuesto =Math.Abs((Convert.ToDecimal(row[6]))); //Math.Round(ven.Suma - ven.SubTotalVenta, 2);
                                    ven.TotalVenta =(ven.Suma);
                                    ven.TipoPrecio = "01";
                                    ven.UnidadCliente = Convert.ToString(row[3]).Trim();
                                    if (ven.Impuesto != 0)
                                    {
                                        ven.TipoImpuesto = "10";
                                    }
                                    else
                                    {
                                        ven.TipoImpuesto = "20"; //10 operacion onerosa - 20 exonerada
                                    }
                                }
                                else if (Convert.ToString(row[1]).Trim() == "TXT")
                                {
                                    ven.Descripcion += "\n" + Convert.ToString(row[2]).Trim();

                                }

                                i++;
                                if (dt_DetalleVenta.Rows.Count == i) Items.Add(ven);
                            }    
                        }
                    }
                    _documento.Items = Items;
                    //Cliente
                    _documento.Receptor.NroDocumento = CVentas1.NumDocCliente.Trim();
                    _documento.Receptor.NombreLegal = CVentas1.Cliente.Trim();
                    _documento.Receptor.Direccion = CVentas1.DirCliente.Trim();
                    _documento.FechaVencimiento = CVentas1.FechaVencimiento;
                    _documento.FechaEmision = CVentas1.FechaEmision.ToString("yyyy-MM-dd");
                    //Totales
                    CalcularTotales();

                    string str1 = Convert.ToString(CVentas1.Serie); //aqui esta el problema
                    string str2 = Convert.ToString(CVentas1.Numeracion);
                    char pad = '0';
                    /*NC - ND*/
                    String NuevaSerie = "",NuevoTipoDocumento="";
                    if (CVentas1.SiglaDocAfecta.Trim() == "FT")
                    {

                        NuevaSerie = "FE01";
                        NuevoTipoDocumento = "01";
                    }
                    else if (CVentas1.SiglaDocAfecta.Trim() == "BV")
                    {

                        NuevaSerie = "BE01";
                        NuevoTipoDocumento = "03";

                    }
                    /*Fin NC - ND*/
                    switch (CVentas1.Sigla) {
                        case "FT":
                            //_documento.IdDocumento ="FE" + str1.PadLeft(2, pad).Trim() + "-" + str2.PadLeft(8, pad).Trim();
                            _documento.IdDocumento = CVentas1.Serie + "-" + str2.PadLeft(8, pad).Trim();
                            _documento.TipoDocumento = "01";                            
                            break;
                        case "BV":
                            //_documento.IdDocumento = "BE" +str1.PadLeft(2, pad).Trim() +"-" + str2.PadLeft(8, pad).Trim();
                            _documento.IdDocumento = CVentas1.Serie + "-" + str2.PadLeft(8, pad).Trim();
                            _documento.TipoDocumento = "03";
                            break;
                        case "NC": 
                            _documento.IdDocumento = str1.PadLeft(2, pad).Trim() + "-" + str2.PadLeft(8, pad).Trim();
                            _documento.TipoDocumento = "07";
                            _documento.Relacionados.Add(new DocumentoRelacionado { NroDocumento= NuevaSerie +"-" + CVentas1.NumDocAfecta.Trim().PadLeft(8, pad).Trim(), TipoDocumento=NuevoTipoDocumento });
                            _documento.Discrepancias.Add(new Discrepancia { Tipo="01", Descripcion="ANULACION DE DOCUMENTO", NroReferencia= NuevaSerie + "-" + CVentas1.NumDocAfecta.Trim().PadLeft(8, pad) });
                            break;
                        case "ND":
                            _documento.IdDocumento =  str1.PadLeft(2, pad).Trim() +"-" + str2.PadLeft(8, pad).Trim();
                            _documento.TipoDocumento = "08";
                            _documento.Relacionados.Add(new DocumentoRelacionado { NroDocumento = NuevaSerie + "-" + CVentas1.NumDocAfecta.PadLeft(8, pad).Trim(), TipoDocumento = NuevoTipoDocumento });
                            _documento.Discrepancias.Add(new Discrepancia { Tipo = "03", Descripcion = "OTROS CONCEPTOS", NroReferencia = NuevaSerie + "-" + CVentas1.NumDocAfecta.Trim().PadLeft(8, pad) });
                            break;

                    }
                    

                    switch (_documento.TipoDocumento)
                    {
                        case "07":
                            //NotaCredito
                            var notaCredito = GeneradorXML.GenerarCreditNote(_documento);
                            var serializador1 = new Serializador();
                            TramaXmlSinFirma = serializador1.GenerarXml(notaCredito);
                            break;
                        case "08":
                            //GenerarNotaDebito
                            var notaDebito = GeneradorXML.GenerarDebitNote(_documento);
                            var serializador2 = new Serializador();
                            TramaXmlSinFirma = serializador2.GenerarXml(notaDebito);
                            break;
                        default:
                            var invoice = GeneradorXML.GenerarInvoice(_documento);
                            var serializador3 = new Serializador();
                            TramaXmlSinFirma = serializador3.GenerarXml(invoice);
                            break;
                    }

                    
                    RutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documentos\\" +
                    $"{_documento.IdDocumento}.xml");
                    File.WriteAllBytes(RutaArchivo, Convert.FromBase64String(TramaXmlSinFirma));
                    //btnEnvioSunat.Enabled = true;
                    btnEnvioSunat.Enabled = (CVentas.Sigla == "BV") ? false : true;
                    lblmensaje.Text="Archivo generado correctamente";
                    lblmensaje.Visible = true;
                    Proceso = 1;

                }
                else {
                    MessageBox.Show("Seleccione un registro..!");  
                }
            }
            catch (Exception a ) { MessageBox.Show(a.Message); }
            finally
            {
                btnGeneraXML.Enabled = true;
                Cursor.Current = Cursors.Default;
            }
        }

        private void dgListadoVentas_RowStateChanged(object sender, DataGridViewRowStateChangedEventArgs e)
        {
            try {
                if (dgListadoVentas.Rows.Count >= 1 && e.Row.Selected)
                {
                    CVentas.Sigla= e.Row.Cells[sigla.Name].Value.ToString();
                    CVentas.Serie= e.Row.Cells[serie.Name].Value.ToString();
                    CVentas.Numeracion= e.Row.Cells[numeracion.Name].Value.ToString();

                }
            }
            catch (Exception a) { MessageBox.Show(a.Message); }
        }

        private void btnEnvioSunat_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (!AccesoInternet()) {
                    MessageBox.Show("No hay conexión con el servidor \n Verifique si existe conexión a internet e intente nuevamente.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblmensaje.Visible = false;
                    return;
                }

                if (Proceso == 0) {

                    MessageBox.Show("Debe generar el documento XML para enviar a SUNAT", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    lblmensaje.Visible = false;
                    return;
                }

                if (string.IsNullOrEmpty(_documento.IdDocumento))
                    throw new InvalidOperationException("La Serie y el Correlativo no pueden estar vacíos");

                var tramaXmlSinFirma = Convert.ToBase64String(File.ReadAllBytes(RutaArchivo));

                var firmadoRequest = new FirmadoRequest
                {
                    TramaXmlSinFirma = tramaXmlSinFirma,
                    CertificadoDigital = Convert.ToBase64String(File.ReadAllBytes(recursos + "\\INTEROCEANICAPFX.pfx")),
                    PasswordCertificado = "uY9eYH8utq4SyreY", //546IUYJHGT5
                    UnSoloNodoExtension = false //rbRetenciones.Checked || rbResumen.Checked

                };


                FirmarController enviar = new FirmarController();

                var respuestaFirmado = enviar.FirmadoResponse(firmadoRequest);

                if (!respuestaFirmado.Exito)
                    throw new ApplicationException(respuestaFirmado.MensajeError);

                var oContribuyente = LeerEmpresa(cboEmpresaDoc.SelectedValue.ToString());

                var enviarDocumentoRequest = new EnviarDocumentoRequest
                {
                    Ruc = cboEmpresaDoc.SelectedValue.ToString(),
                    //UsuarioSol = "FACTURA1",
                    //ClaveSol = "FACTURA1",
                    UsuarioSol = oContribuyente.UsuarioSol,
                    ClaveSol = oContribuyente.ClaveSol,
                    EndPointUrl = SunatFact,// ValorSeleccionado(),
                    //https://e-beta.sunat.gob.pe/ol-ti-itcpfegem-beta/billService //RETENCION
                    //https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService
                    IdDocumento = _documento.IdDocumento,
                    TipoDocumento = _documento.TipoDocumento,
                    TramaXmlFirmado = respuestaFirmado.TramaXmlFirmado
                };



                // RespuestaComun respuestaEnvio;
                var respuestaEnvio = new EnviarDocumentoResponse();
               
                EnviarDocumentoController enviarDoc = new EnviarDocumentoController();
                respuestaEnvio = enviarDoc.EnviarDocumentoResponse(enviarDocumentoRequest);

                
                // var rpta =new EnviarDocumentoResponse() ;//(EnviarDocumentoResponse)respuestaEnvio;
                var rpta = (EnviarDocumentoResponse)respuestaEnvio;
                //txtResult.Text = $@"{Resources.procesoCorrecto}{Environment.NewLine}{rpta.MensajeRespuesta} siendo las {DateTime.Now}";
                MessageBox.Show( rpta.MensajeRespuesta+ " Siendo las " + DateTime.Now);
                try
                {
                   
                    if (rpta.Exito && !string.IsNullOrEmpty(rpta.TramaZipCdr))
                    {
                        File.WriteAllBytes($"{Program.CarpetaXml}\\{rpta.NombreArchivo}.xml",
                            Convert.FromBase64String(respuestaFirmado.TramaXmlFirmado));

                        File.WriteAllBytes($"{Program.CarpetaCdr}\\R-{rpta.NombreArchivo}.zip",
                            Convert.FromBase64String(rpta.TramaZipCdr));
                        _documento.FirmaDigital = respuestaFirmado.ValorFirma;
                        lblmensaje.Text = rpta.MensajeRespuesta;
                        GeneraPDF();
                    }
                    //Actualiza Estado
                    CVentas1.NumDocEmisor = oContribuyente.NroDocumento;
                    CVentas1.CodigoRespuesta = rpta.CodigoRespuesta;
                    CVentas1.MensajeRespuesta = rpta.MensajeRespuesta;
                    CVentas1.NombreArchivo = rpta.NombreArchivo+".xml";
                    CVentas1.NombreArchivoCDR = "R-" + rpta.NombreArchivo + ".zip";
                    CVentas1.NombreArchivoPDF = _documento.Emisor.NroDocumento + "-" + DateTime.Parse(_documento.FechaEmision).ToString("yyyy-MM-dd") + "-" + _documento.IdDocumento+".pdf";
                    if (rpta.CodigoRespuesta == "0") { //Aceptado
                       
                        if (CVentas1 != null && CVentas1.Numeracion != "") {
                            CVentas1.EstadoDocSunat = 3;                         
                            AdmCVenta.update(CVentas1);
                        } 
                    }
                    else if (rpta.CodigoRespuesta == null)
                    {
                        var msg = string.Concat(rpta.MensajeRespuesta);
                        var faultCode = "Client.";
                        if (msg.Contains(faultCode))
                        {
                            var posicion = msg.IndexOf(faultCode, StringComparison.Ordinal);
                            var codigoError = msg.Substring(posicion + faultCode.Length, 4);
                            msg = codigoError;
                        }

                        CVentas1.EstadoDocSunat = 1;
                        CVentas1.CodigoRespuesta = msg;
                        AdmCVenta.update(CVentas1);
                    }
                    CargaVentas();
                }
                catch (Exception ex)
                {
                    lblmensaje.Visible=false;
                    MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                

                if (!respuestaEnvio.Exito)
                    throw new ApplicationException(respuestaEnvio.MensajeError);
                lblmensaje.Visible = false;


            }
            catch (Exception ex)
            {
             
                MessageBox.Show(ex.Message);
                lblmensaje.Visible = false;
            }
            finally
            {
                btnGeneraXML.Enabled = true;
                btnEnvioSunat.Enabled = false;
                Cursor = Cursors.Default;
            }
        }

       
        private void btnGeneraPDF_Click(object sender, EventArgs e)
        {
            try {
                GeneraPDF();
            }
            catch (Exception a ) { MessageBox.Show(a.Message); }
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgListadoVentas_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try {
                //Se valida el tipo de documento
                btnEnvioSunat.Enabled = (dgListadoVentas.CurrentRow.Cells[sigla.Name].Value.ToString() == "BV") ? false : true;
                if (dgListadoVentas.Columns[e.ColumnIndex].Name.Equals("xml"))
                {
                    //Aqui va el code que quieres que realize
                    RutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XML\\" + dgListadoVentas.CurrentRow.Cells[Nomxml.Name].Value.ToString());
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = RutaArchivo;
                    p.Start();
                } else if (dgListadoVentas.Columns[e.ColumnIndex].Name.Equals("cdr")) {
                    
                    RutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CDR\\" + dgListadoVentas.CurrentRow.Cells[Nomcdr.Name].Value.ToString());
                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = RutaArchivo;
                    p.Start();

                } else if (dgListadoVentas.Columns[e.ColumnIndex].Name.Equals("pdf")) {
                   
                    if (dgListadoVentas.CurrentRow.Cells[sigla.Name].Value.ToString()=="FT") {

                        RutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FACTURAS_PDF\\" + dgListadoVentas.CurrentRow.Cells[Nompdf.Name].Value.ToString());

                    } else if (dgListadoVentas.CurrentRow.Cells[sigla.Name].Value.ToString() == "BV") {

                        RutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "BOLETAS_PDF\\" + dgListadoVentas.CurrentRow.Cells[Nompdf.Name].Value.ToString());
                    }
                    else if (dgListadoVentas.CurrentRow.Cells[sigla.Name].Value.ToString() == "NC")
                    {

                        RutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NOTA_CREDITO_PDF\\" + dgListadoVentas.CurrentRow.Cells[Nompdf.Name].Value.ToString());
                    }
                    if (dgListadoVentas.CurrentRow.Cells[sigla.Name].Value.ToString() == "ND")
                    {

                        RutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NOTA_DEBITO_PDF\\" + dgListadoVentas.CurrentRow.Cells[Nompdf.Name].Value.ToString());
                    }


                    System.Diagnostics.Process p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = RutaArchivo;
                    p.Start();
                }
            }
            catch (Exception a ) { MessageBox.Show(a.Message); }
        }

        private void btnFiltrar_Click(object sender, EventArgs e)
        {
            CargaVentas();
        }

        private void txtBuscaCliente_TextChanged(object sender, EventArgs e)
        {
            
        }

        Int32 counter2 = 1;

        private void kryptonButton4_Click(object sender, EventArgs e)
        {
            try {
                if (textBox7.Text == "")
                {
                    MessageBox.Show("Ingrese Serie");
                    textBox7.Focus();
                    return;
                }
                if (textBox1.Text == "")
                {
                    MessageBox.Show("Ingrese Numeración");
                    textBox1.Focus();
                    return;
                }
                if (txtmotivo.Text == "")
                {
                    MessageBox.Show("Ingrese Motivo de Anulación");
                    txtmotivo.Focus();
                    return;
                }
                if (comboBox1.SelectedIndex == 0)
                {
                    CodTipoDocumento = "01";
                }
                else if (comboBox1.SelectedIndex == 1) {
                    CodTipoDocumento = "03";
                }
                dglista2.Rows.Add(counter2, CodTipoDocumento, textBox7.Text, textBox1.Text, txtmotivo.Text);
                counter2++;
                textBox1.Text = "";
                txtmotivo.Text = "";
            } catch (Exception a ) { MessageBox.Show(a.Message); }
        }

        private void kryptonButton3_Click(object sender, EventArgs e)
        {
            try
            {
                if (dglista2.Rows.Count > 0)
                {
                    dglista2.Rows.RemoveAt(dglista2.CurrentRow.Index);

                }
                else
                {
                    MessageBox.Show("No hay registros por eliminar");
                }
            }
            catch (Exception a ) { MessageBox.Show(a.Message); }
        }

        private void kryptonButton5_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                if (dglista2.Rows.Count > 0)
                {
                    if (txtcorrelativo2.Text == "") {

                        MessageBox.Show("Ingrese Correlativo");
                        txtcorrelativo2.Focus();
                        return;
                    }

                    var correl = txtcorrelativo2.Text;
                    var documentoBaja = new ComunicacionBaja
                    {

                        IdDocumento = string.Format("RA-{0:yyyyMMdd}-" + correl, DateTime.Today),
                        FechaEmision = DateTime.Today.ToString("yyyy-MM-dd"),
                        FechaReferencia = FechaEmisionDocBaja.Value.ToString("yyyy-MM-dd"),//DateTime.Today.ToString("yyyy-MM-dd"),//DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
                        //Emisor = CrearEmisor(),
                        Emisor = LeerEmpresa(cboEmpresaBaj.SelectedValue.ToString()),
                        Bajas = new List<DocumentoBaja>()

                    };
                    var nomdoc = "RA-" + string.Format("{0:yyyyMMdd}-" + correl, DateTime.Today);
                    foreach (DataGridViewRow row in dglista2.Rows)
                    {
                        DocumentoBaja baja = new DocumentoBaja();
                        baja.Id = Convert.ToInt32(row.Cells[0].Value);
                        baja.TipoDocumento = Convert.ToString(row.Cells[1].Value);
                        baja.Serie = Convert.ToString(row.Cells[2].Value);
                        baja.Correlativo = Convert.ToString(row.Cells[3].Value);
                        baja.MotivoBaja = Convert.ToString(row.Cells[4].Value);

                        documentoBaja.Bajas.Add(baja);

                    }
                    var invoice = GeneradorXML.GenerarVoidedDocuments(documentoBaja);
                    var serializador3 = new Serializador();
                    TramaXmlSinFirma = serializador3.GenerarXml(invoice);
                    RutaArchivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Documentos\\" +
                     $"{documentoBaja.IdDocumento}.xml");
                    File.WriteAllBytes(RutaArchivo, Convert.FromBase64String(TramaXmlSinFirma));
                    IdDocumento = nomdoc;
                    _documento.IdDocumento = IdDocumento;
                    _documento.TipoDocumento = "RA";
                    if (string.IsNullOrEmpty(_documento.IdDocumento))
                        throw new InvalidOperationException("La Serie y el Correlativo no pueden estar vacíos");

                    var tramaXmlSinFirma = Convert.ToBase64String(File.ReadAllBytes(RutaArchivo));

                    var firmadoRequest = new FirmadoRequest
                    {
                        TramaXmlSinFirma = tramaXmlSinFirma,
                        CertificadoDigital = Convert.ToBase64String(File.ReadAllBytes(recursos + "\\INTEROCEANICAPFX.pfx")),
                        PasswordCertificado = "uY9eYH8utq4SyreY", //546IUYJHGT5
                        UnSoloNodoExtension = true //rbRetenciones.Checked || rbResumen.Checked

                    };


                    FirmarController enviar = new FirmarController();

                    var respuestaFirmado = enviar.FirmadoResponse(firmadoRequest);

                    if (!respuestaFirmado.Exito)
                        throw new ApplicationException(respuestaFirmado.MensajeError);

                    var oContribuyente = LeerEmpresa(cboEmpresaBaj.SelectedValue.ToString());

                    var enviarDocumentoRequest = new EnviarDocumentoRequest
                    {
                        Ruc = cboEmpresaBaj.SelectedValue.ToString(),
                        //UsuarioSol = "FACTURA1",
                        //ClaveSol = "FACTURA1",
                        UsuarioSol = oContribuyente.UsuarioSol,
                        ClaveSol = oContribuyente.ClaveSol,
                        EndPointUrl = SunatFact,// ValorSeleccionado(),
                        //https://e-beta.sunat.gob.pe/ol-ti-itemision-otroscpe-gem-beta/billService //RETENCION
                        //https://www.sunat.gob.pe:443/ol-ti-itemision-otroscpe-gem/billService
                        IdDocumento = _documento.IdDocumento,
                        TipoDocumento = _documento.TipoDocumento,
                        TramaXmlFirmado = respuestaFirmado.TramaXmlFirmado
                    };
                    var respuestaEnvio = new EnviarDocumentoResponse();
                    EnviarResumenController enviaResumen = new EnviarResumenController();
                    respuestaEnvio = enviaResumen.EnviarResumenResponse(enviarDocumentoRequest);


                    var rpta = (EnviarDocumentoResponse)respuestaEnvio;
                    txtResult.Text = $@"{Resources.procesoCorrecto}{Environment.NewLine}{rpta.NroTicket}";
                    if (rpta.Exito) txtNroTicket.Text = rpta.NroTicket.ToString();
                    if (!respuestaEnvio.Exito)
                        throw new ApplicationException(respuestaEnvio.MensajeError);


                    DialogResult = DialogResult.OK;
                }
                else
                {
                    MessageBox.Show("No hay Registros para Generar Documento");
                    return;
                }
                

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void kryptonButton7_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;               
                   
                if (string.IsNullOrEmpty(txtNroTicket.Text)) return;

                var oContribuyente = LeerEmpresa(cboEmpresaBaj.SelectedValue.ToString());

                var consultaTicketRequest = new ConsultaTicketRequest
                {
                    //Ruc = "20513258934",
                    //UsuarioSol = "FACTURA1",
                    //ClaveSol = "FACTURA1",
                    Ruc = oContribuyente.NroDocumento,
                    UsuarioSol = oContribuyente.UsuarioSol,
                    ClaveSol = oContribuyente.ClaveSol,
                    //EndPointUrl = "https://e-factura.sunat.gob.pe/ol-ti-itcpfegem/billService",// ValorSeleccionado(),
                    EndPointUrl = SunatFact,
                    IdDocumento = IdDocumento,
                    NroTicket = txtNroTicket.Text
                };
                var respuestaEnvio = new EnviarDocumentoResponse();
                ConsultarTicket ConsultaTiket = new ConsultarTicket();
                respuestaEnvio = ConsultaTiket.EnviarDocumentoResponse(consultaTicketRequest);

                if (!respuestaEnvio.Exito)
                    throw new ApplicationException(respuestaEnvio.MensajeError);

                txtResult.Text = $"{Resources.procesoCorrecto}{Environment.NewLine}{respuestaEnvio.MensajeRespuesta}";

              
            }
            catch (Exception ex)
            {
                txtResult.Text = ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void kryptonButton6_Click(object sender, EventArgs e)
        {
            var documentoBaja = new ComunicacionBaja
            {

                IdDocumento = "",
                FechaEmision = DateTime.Today.ToString("yyyy-MM-dd"),
                FechaReferencia = "",//DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd"),
                //Emisor = CrearEmisor(),
                Emisor = LeerEmpresa(cboEmpresaDoc.SelectedValue.ToString()),
                Bajas = new List<DocumentoBaja>()

            };
            documentoBaja.Bajas.Clear();

            comboBox1.SelectedIndex = -1;
            textBox1.Text = "";
            txtmotivo.Text = "";
            dglista2.Rows.Clear();
            txtResult.Text = "";
            txtNroTicket.Text = "";
        }

        private void btnSendResumen_Click(object sender, EventArgs e)
        {
            try {
                Cursor.Current = Cursors.WaitCursor;
                EnviarResumen();

            } catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            } finally
            {

            }
        }

        private void btnConsultarRes_Click(object sender, EventArgs e)
        {
            CargaBoletas();
        }

        private void btnConsultarResumen_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                if (string.IsNullOrEmpty(txtNumTicketResumen.Text)) return;

                var oContribuyente = LeerEmpresa(cboEmpresa.SelectedValue.ToString());

                var consultaTicketRequest = new ConsultaTicketRequest
                {
                    Ruc = cboEmpresa.SelectedValue.ToString(),
                    //UsuarioSol = "FACTURA1",
                    //ClaveSol = "FACTURA1",
                    UsuarioSol = oContribuyente.UsuarioSol,
                    ClaveSol = oContribuyente.ClaveSol,
                    EndPointUrl = SunatFact,
                    IdDocumento = IdDocumento,
                    NroTicket = txtNumTicketResumen.Text
                };
                var respuestaEnvio = new EnviarDocumentoResponse();
                ConsultarTicket ConsultaTiket = new ConsultarTicket();
                respuestaEnvio = ConsultaTiket.EnviarDocumentoResponse(consultaTicketRequest);

                if (!respuestaEnvio.Exito)
                {
                    throw new ApplicationException(respuestaEnvio.MensajeError);
                }
                else
                {
                    AdmCVenta.ActualizarEstadoResumen(oContribuyente.NroDocumento, txtNumTicketResumen.Text);
                    txtDetailRes.Text = $"{Resources.procesoCorrecto}{Environment.NewLine}{respuestaEnvio.MensajeRespuesta}";
                    CargaBoletas();
                }
            }
            catch (Exception ex)
            {
                txtDetailRes.Text = ex.Message;
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnDelBol_Click(object sender, EventArgs e)
        {
            if (grvResDetail.CurrentRow != null)
            {
                DataGridViewRow row = grvResDetail.Rows[grvResDetail.CurrentRow.Index];
                var numdoc = row.Cells[0].Value.ToString();
                if (MessageBox.Show("¿Está seguro de Anular el documento " + numdoc + "?", "Anlar Boleta", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    grvResDetail.Rows.RemoveAt(grvResDetail.CurrentRow.Index);
                    var tipdoc = numdoc.Substring(0, 2);
                    var sersun = numdoc.Substring(2, 4);
                    var numsun = numdoc.Substring(6, 7);

                    Boolean resp = AdmCEmpresa.AnularDocumento(cboEmpresa.SelectedValue.ToString(), tipdoc, sersun, numsun);
                    if (resp)
                    {
                        MessageBox.Show("Se ha anulado el documento correctamente", "Correcto", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Ocurrio un error al anular el documento", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {

                }
            }
            else
            {
                MessageBox.Show("No se ha seleccionado ninguna Boleta para Anular");
            }
        }

        private void btnConsultarRuc_Click(object sender, EventArgs e)
        {
            try
            {
                bool valida = true;
                if (txtnumruc.Text.Length != 11)
                {
                    valida = false;
                    MessageBox.Show("El numero de ruc debe contener 11 digitos");
                }
                /*if (!ValidaRuc(Convert.ToInt64(txtnumruc.Text)) && valida) {
                    valida = false;
                    MessageBox.Show("El numero de ruc es invalido");
                }*/

                if (valida) {
                    WebRequest request = WebRequest.Create(String.Format("http://wmtechnology.org/Consultar-RUC/?modo=1&btnBuscar=Buscar&nruc={0}", txtnumruc.Text));
                    request.Method = "POST";
                    WebResponse response = request.GetResponse();
                    StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding("ISO-8859-1"));

                    String sunathtml = sr.ReadToEnd();
                    sunathtml = sunathtml.Trim();
                    //Se prepara el documento
                    sunathtml = Regex.Replace(sunathtml, @"<!--.*?-->", ""); //Se elimina comentarios
                    sunathtml = Regex.Replace(sunathtml, @"<meta.*?>", ""); //Se elimina etiquetas meta
                    sunathtml = Regex.Replace(sunathtml, @"<link.*?>", ""); //Se elimina etiquetas link
                    sunathtml = Regex.Replace(sunathtml, @"&copy.*?;", ""); //Se elimina texto copy
                    sr.Close();
                    XmlDocument doc = new XmlDocument();
                    //Se carga el XML preparado
                    doc.LoadXml(sunathtml);
                    //Se selecciona el nodo con la información
                    XmlNodeList xnList = doc.SelectNodes("/html/body/div[@class='container']/div/div/div[@class='panel panel-primary']/div[@class='list-group']");
                    foreach (XmlNode xn in xnList)
                    {
                        doc.LoadXml(String.Format("<root>{0}</root>", xn.InnerXml));
                    }
                    //Se recuperan los datos del RUC solicitado
                    String[] emisor = doc.SelectSingleNode("/root/div[1]/div/div[2]/h4").InnerText.Split('-');
                    String numruc = emisor[0].ToString().Trim();
                    String razsoc = emisor[1].ToString().Trim();
                    String estado = doc.SelectSingleNode("/root/div[2]/div/div[2]/p").InnerText.Trim();
                    String condic = doc.SelectSingleNode("/root/div[3]/div/div[2]/p").InnerText.Trim();
                    String ubigeo = doc.SelectSingleNode("/root/div[4]/div/div[2]").InnerText.Trim();
                    String nomdep = doc.SelectSingleNode("/root/div[5]/div/div[2]").InnerText.Trim();
                    String nomprv = doc.SelectSingleNode("/root/div[6]/div/div[2]").InnerText.Trim();
                    String nomdis = doc.SelectSingleNode("/root/div[7]/div/div[2]").InnerText.Trim();
                    String domfis = doc.SelectSingleNode("/root/div[8]/div/div[2]/p").InnerText.Trim();

                    //Se llenan los datos en el formulario
                    txtrazsoc.Text = razsoc;
                    txtestemi.Text = estado;
                    txtconemi.Text = condic;
                    txtubigeo.Text = ubigeo;
                    txtnomdep.Text = nomdep;
                    txtnomprv.Text = nomprv;
                    txtnomdis.Text = nomdis;
                    txtdomfis.Text = domfis;
                }
                else
                {
                    txtrazsoc.Clear();
                    txtestemi.Clear();
                    txtconemi.Clear();
                    txtubigeo.Clear();
                    txtnomdep.Clear();
                    txtnomprv.Clear();
                    txtnomdis.Clear();
                    txtdomfis.Clear();
                }
            }
            catch(Exception ex)
            {
                txtrazsoc.Clear();
                txtestemi.Clear();
                txtconemi.Clear();
                txtubigeo.Clear();
                txtnomdep.Clear();
                txtnomprv.Clear();
                txtnomdis.Clear();
                txtdomfis.Clear();

                MessageBox.Show("Error en la petición: " + ex.Message.ToString());


            }
        }

        private void txtnumruc_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) {
                btnConsultarRuc.PerformClick();
            }
        }

        private void btnConectarServer_Click(object sender, EventArgs e)
        {
            BaseDatos bdemi = new BaseDatos(txtserver.Text, BaseDatos.BBDD.SQL, "master", txtuser.Text, txtpass.Text);
            try {
                bdemi.Conectar();
                DataTable dt_basedatos = new DataTable();
                bdemi.Dame_Datos_DT("SELECT NAME, DATABASE_ID FROM sys.databases", false, ref dt_basedatos, "S");
                bdemi.Desconectar();
                cboBaseDatos.DataSource = dt_basedatos;
                cboBaseDatos.ValueMember = "DATABASE_ID";
                cboBaseDatos.DisplayMember = "NAME";

                MessageBox.Show("Conexión creada correctamente");
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error al conectar: " + ex.Message.ToString());
            }
        }

        private void cboBaseDatos_SelectedIndexChanged(object sender, EventArgs e)
        {

            loadTables();

        }

        private void btnSaveEmisor_Click(object sender, EventArgs e)
        {
            if (txtrazsoc.Text == "") {
                MessageBox.Show("Debe ingresar un RUC y realizar la busqueda");
                return;
            }
            if (txtCodAge.Text == "")
            {
                MessageBox.Show("Debe ingresar un codigo de agencia");
                return;
            }
            if (txtserver.Text == "") {
                MessageBox.Show("Debe ingresar el nombre del servidor de base de datos");
                return;
            }
            if (txtuser.Text == "")
            {
                MessageBox.Show("Debe ingresar el usuario de base de datos");
                return;
            }
            if (txtpass.Text == "")
            {
                MessageBox.Show("Debe ingresar la clave de base de datos");
                return;
            }
            if (cboBaseDatos.Items.Count == 0) {
                MessageBox.Show("No existe ninguna base de datos para conectar");
                return;
            }

            if (btnCancelEditEmi.Visible)
            {
                if (ActualizarEmisor())
                {
                    MessageBox.Show("Se ha actualizado correctamente el emisor electronico");
                }
            }
            else
            {
                if (GuardarEmisor())
                {
                    MessageBox.Show("Se ha guardado correctamente el emisor electronico");
                };
            }

            CargaEmpresa();
        }

        private void btnEditarEmisor_Click(object sender, EventArgs e)
        {
            if (grvEmisores.CurrentRow != null)
            {
                btnCancelEditEmi.Visible = true;
                DataGridViewRow row = grvEmisores.Rows[grvEmisores.CurrentRow.Index];
                DataTable dt_empresa = AdmCEmpresa.CargaEmpresa(row.Cells[0].Value.ToString());
                String basedatos = "", tablacab = "", tabladet = "";
                foreach(DataRow dtrow in dt_empresa.Rows)
                {
                    txtnumruc.Text = dtrow["NU_EMINUMRUC"].ToString();
                    txtCodAge.Text = dtrow["CO_EMICODAGE"].ToString();
                    txtusersun.Text = dtrow["NO_USUSOLSUN"].ToString();
                    txtpasssun.Text = dtrow["NO_PASSOLSUN"].ToString();
                    txtserver.Text = dtrow["NO_BASNOMSRV"].ToString();
                    txtuser.Text = dtrow["NO_BASUSRBAS"].ToString();
                    txtpass.Text = dtrow["NO_BASUSRPAS"].ToString();
                    basedatos = dtrow["NO_BASNOMBAS"].ToString();
                    tablacab = dtrow["NO_TABFACCAB"].ToString();
                    tabladet = dtrow["NO_TABFACDET"].ToString();
                    cboEstadoEmisor.SelectedIndex = Convert.ToInt32(dtrow["FL_REGINACTI"].ToString());
                }
                btnConsultarRuc.PerformClick();
                btnConectarServer.PerformClick();
            }
            else
            {
                MessageBox.Show("No se ha seleccionado ningun registro ...!!");
            }
        }

        private void btnCancelEditEmi_Click(object sender, EventArgs e)
        {
            txtnumruc.Clear();
            txtrazsoc.Clear();
            txtestemi.Clear();
            txtconemi.Clear();
            txtubigeo.Clear();
            txtnomdep.Clear();
            txtnomprv.Clear();
            txtnomdis.Clear();
            txtdomfis.Clear();
            txtCodAge.Clear();
            txtusersun.Clear();
            txtpasssun.Clear();
            txtserver.Clear();
            txtuser.Clear();
            txtpass.Clear();

            DataTable nothing = new DataTable();
            cboBaseDatos.DataSource = nothing;
            cboBaseDatos.DisplayMember = null;
            cboBaseDatos.ValueMember = null;

            cboTablaCab.DataSource = nothing;
            cboTablaCab.DisplayMember = null;
            cboTablaCab.ValueMember = null;

            cboTablaDet.DataSource = nothing;
            cboTablaDet.DisplayMember = null;
            cboTablaDet.ValueMember = null;

            btnCancelEditEmi.Visible = false;
        }
    }
}
