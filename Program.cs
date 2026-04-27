using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Configuration;
using System.Windows.Forms;
using System.Security.Principal;
using ABB_Actualiza_2008.dal;

namespace ABB_Actualiza_2008
{
	internal class Program
	{
		private static string FormatearSqlException(SqlException ex)
		{
			StringBuilder sb = new StringBuilder();
			foreach (SqlError e in ex.Errors)
			{
				sb.AppendLine(String.Format("[SQL {0}] {1} (Proc: {2}, Line: {3}, Server: {4})", e.Number, e.Message, e.Procedure, e.LineNumber, e.Server));
			}
			return sb.ToString();
		}

		/// <summary>
		/// Normaliza los nombres de columnas del DataTable origen (SQL 6.5 via ODBC)
		/// para que coincidan con los nombres de la tabla destino SQL Server 2008.
		/// Esto evita errores de ColumnMapping (Column1/Column2) y typos (CATA_NOME, PREP_AUTO, es_hilo).
		/// </summary>
		private static void NormalizarColumnasCatalogosSap(DataTable dt)
		{
			if (dt == null) return;

			// Typos / nombres distintos entre origen y destino
			if (dt.Columns.Contains("CATA_NOME") && !dt.Columns.Contains("CATA_NUME"))
				dt.Columns["CATA_NOME"].ColumnName = "CATA_NUME";

			if (dt.Columns.Contains("PREP_AUTO") && !dt.Columns.Contains("PREPA_AUTO"))
				dt.Columns["PREP_AUTO"].ColumnName = "PREPA_AUTO";

			if (dt.Columns.Contains("es_hilo") && !dt.Columns.Contains("es_hijo"))
				dt.Columns["es_hilo"].ColumnName = "es_hijo";

			// Column1 / Column2 vienen típicamente de expresiones CONVERT(...) sin alias en ODBC.
			// Según tu SELECT origen:
			//   Column1 => convert(char(18), ANTE_CATA)
			//   Column2 => convert(char(18), ANTE_CATA_NUME)
			if (dt.Columns.Contains("Column1") && !dt.Columns.Contains("ANTE_CATA"))
				dt.Columns["Column1"].ColumnName = "ANTE_CATA";

			if (dt.Columns.Contains("Column2") && !dt.Columns.Contains("ANTE_CATA_NUME"))
				dt.Columns["Column2"].ColumnName = "ANTE_CATA_NUME";
		}

		private static void Main(string[] args)
		{
			WindowsIdentity wi = WindowsIdentity.GetCurrent();
			WindowsPrincipal prin = new WindowsPrincipal(wi);

			string userConection = prin.Identity.Name.ToString();
			userConection = userConection.Substring(userConection.IndexOf('\\') + 1);

			string connectionString = ConfigurationSettings.AppSettings["Database.Default"].ToString();
			connectionString += ConfigurationSettings.AppSettings["Database.Default_2"].ToString();
			string strConexion2008 = ConfigurationSettings.AppSettings["Database.Default_2008"].ToString();

			DataTable dataTable = new DataTable();
			DataTable table2 = new DataTable();

			DBManager administradorBaseDatos = new DBManager(
				DataProvider.SqlServer,
				ConfigurationSettings.AppSettings["Database.Default_2008"].ToString()
			);

			ApplicationException errores = null;
			string textoErrores = string.Empty;
			string textoAciertos = string.Empty;

			try
			{
				// =========================
				// 1) Traer datos desde 6.5 (ODBC)
				// =========================
				try
				{
					using (OdbcConnection connection = new OdbcConnection(connectionString))
					{
						OdbcDataAdapter adapter1 = new OdbcDataAdapter("ABB_2008_CATALOGOS_SAP_CONSUL", connection);
						adapter1.Fill(dataTable);

						// Origen para diagnóstico
						dataTable.TableName = "SQL65: ABB_2008_CATALOGOS_SAP_CONSUL";

						// Normalizar nombres de columnas para que matcheen BCP_Catalogos_SAP
						NormalizarColumnasCatalogosSap(dataTable);

						textoAciertos = "\n La consulta a CATALOGOS_SAP desde el 6.5 fue exitosa.";
						errores = new ApplicationException(textoAciertos);

						OdbcDataAdapter adapter2 = new OdbcDataAdapter("ABB_2008_EMPRESAS_CONSUL", connection);
						adapter2.Fill(table2);

						// Origen para diagnóstico
						table2.TableName = "SQL65: ABB_2008_EMPRESAS_CONSUL";

						textoAciertos = "\n La consulta a EMPRESAS desde el 6.5 fue exitosa.";
						if (errores != null)
						{
							textoAciertos = errores.Message + textoAciertos;
						}
						errores = new ApplicationException(textoAciertos);
					}
				}
				catch (Exception ex)
				{
					textoErrores = "\n Hubo problemas en traer datos desde el 6.5: \n" + ex.ToString();
					if (errores != null)
					{
						textoErrores = errores.Message + textoErrores;
					}
					errores = new ApplicationException(textoErrores, ex);
					throw errores;
				}

				// =========================
				// 2) Catalogos SAP: limpiar -> bulk insert -> SP
				// =========================
				try
				{
					try
					{
						DataSetsRetriever.CleanTemporalTable("ABB_BCP_CATALOGOS_SAP_LIMPIAR", strConexion2008);

						textoAciertos = "\n La limpieza de BCP_CATALOGOS_SAP fue exitosa.";
						if (errores != null)
						{
							textoAciertos = errores.Message + textoAciertos;
						}
						errores = new ApplicationException(textoAciertos);
					}
					catch (SqlException ex)
					{
						textoErrores = "\n Hubo error SQL al borrar la tabla BCP_CATALOGOS_SAP: \n" + FormatearSqlException(ex);
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}
					catch (Exception ex)
					{
						textoErrores = "\n Hubo problemas para borrar la tabla BCP_CATALOGOS_SAP: \n" + ex.ToString();
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}

					// Bulk insert
					try
					{
						DataSetsRetriever.BulkInsert(dataTable, "BCP_CATALOGOS_SAP", strConexion2008);

						textoAciertos = "\n La carga de BCP_CATALOGOS_SAP fue exitosa.";
						if (errores != null)
						{
							textoAciertos = errores.Message + textoAciertos;
						}
						errores = new ApplicationException(textoAciertos);
					}
					catch (SqlException ex)
					{
						textoErrores = "\n Hubo error SQL en la carga de BCP_CATALOGOS_SAP: \n" + FormatearSqlException(ex);
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}
					catch (Exception ex)
					{
						textoErrores = "\n Hubo error GENERAL en la carga de BCP_CATALOGOS_SAP: \n" + ex.ToString();
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}

					// Transferencia
					try
					{
						administradorBaseDatos.ExecuteNonQuery(CommandType.StoredProcedure, "ABB_CATALOGOS_SAP_DEMON_ACTU");

						textoAciertos = "\n La transferencia de BCP_CATALOGOS_SAP a CATALOGOS_SAP fue exitosa.";
						if (errores != null)
						{
							textoAciertos = errores.Message + textoAciertos;
						}
						errores = new ApplicationException(textoAciertos);
					}
					catch (SqlException ex)
					{
						textoErrores = "\n Hubo error SQL en el pasaje de BCP_CATALOGOS_SAP a CATALOGOS_SAP: \n" + FormatearSqlException(ex);
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}
					catch (Exception ex)
					{
						textoErrores = "\n Hubo error GENERAL en el pasaje de BCP_CATALOGOS_SAP a CATALOGOS_SAP: \n" + ex.ToString();
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}
				}
				catch (ApplicationException)
				{
					throw;
				}

				// =========================
				// 3) Empresas: limpiar -> bulk insert -> SP
				// =========================
				try
				{
					try
					{
						DataSetsRetriever.CleanTemporalTable("ABB_BCP_EMPRESAS_LIMPIAR", strConexion2008);

						textoAciertos = "\n La limpieza de BCP_Empresas fue exitosa";
						if (errores != null)
						{
							textoAciertos = errores.Message + textoAciertos;
						}
						errores = new ApplicationException(textoAciertos);
					}
					catch (SqlException ex)
					{
						textoErrores = "\n Hubo error SQL al borrar la tabla BCP_EMPRESAS: \n" + FormatearSqlException(ex);
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}
					catch (Exception ex)
					{
						textoErrores = "\n Hubo problemas para borrar la tabla BCP_EMPRESAS: \n" + ex.ToString();
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}

					// Bulk insert
					try
					{
						DataSetsRetriever.BulkInsert(table2, "BCP_EMPRESAS", strConexion2008);

						textoAciertos = "\n Carga de BCP_EMPRESAS fue exitosa.";
						if (errores != null)
						{
							textoAciertos = errores.Message + textoAciertos;
						}
						errores = new ApplicationException(textoAciertos);
					}
					catch (SqlException ex)
					{
						textoErrores = "\n Hubo error SQL en la carga de BCP_EMPRESAS: \n" + FormatearSqlException(ex);
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}
					catch (Exception ex)
					{
						textoErrores = "\n Hubo error GENERAL en la carga de BCP_EMPRESAS: \n" + ex.ToString();
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}

					// Transferencia
					try
					{
						administradorBaseDatos.ExecuteNonQuery(CommandType.StoredProcedure, "ABB_EMPRESAS_DEMON_ACTUALIZADOR");

						textoAciertos = "\n La transferencia de BCP_EMPRESAS a EMPRESAS fue exitosa.";
						if (errores != null)
						{
							textoAciertos = errores.Message + textoAciertos;
						}
						errores = new ApplicationException(textoAciertos);
					}
					catch (SqlException ex)
					{
						textoErrores = "\n Hubo error SQL en el pasaje de BCP_EMPRESAS a EMPRESAS: \n" + FormatearSqlException(ex);
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}
					catch (Exception ex)
					{
						textoErrores = "\n Hubo error GENERAL en el pasaje de BCP_EMPRESAS a EMPRESAS: \n" + ex.ToString();
						if (errores != null)
						{
							textoErrores = errores.Message + textoErrores;
						}
						errores = new ApplicationException(textoErrores, ex);
						throw errores;
					}
				}
				catch (ApplicationException)
				{
					throw;
				}

				if (errores != null)
				{
					// Ojo: en tu diseño 'errores' también guarda aciertos.
					// Si querés solo lanzar cuando hubo error real, habría que separar "mensajes" de "errores".
					throw errores;
				}
			}
			catch (ApplicationException erroresVarios)
			{
				administradorBaseDatos.CreateParameters(3);
				administradorBaseDatos.AddParameters(0, "@nombreDemonio", "ABB_Actualiza_2008");
				administradorBaseDatos.AddParameters(1, "@usuario", userConection);
				administradorBaseDatos.AddParameters(2, "@mensajes", erroresVarios.Message);
				administradorBaseDatos.ExecuteNonQuery(CommandType.StoredProcedure, "ABB_LogErrores_Insert");

				MessageBox.Show(erroresVarios.Message);
			}    
		}
	}
}