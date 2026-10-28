using Hydra;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;
using System.Data.SqlClient;
using System.Data;
using System;
using cdn_api;
using System.Collections.Generic;
using static WMnaZZ.Doki;
using System.Text;
using WMnaZZ;

[assembly: CallbackAssemblyDescription(Name: "ONE-041 WMnaZZ",
                                           Description: "Procedura generowania dokumentów ZZ na podstawie WM.",
                                           Author: "OneSupport",
                                           Version: "1.0",
                                           SystemVersion: "2023.2.0",
                                           Created: "19-04-2024")]
namespace WMnaZZ
{
    [SubscribeProcedure(Procedures.MaN_WM, "WMnaZZ")]
    public class DodajFakture : Callback
    {
        private long gidNumer = 0;
        private long gidTyp = 0;
        private long gidFirma = 0;
        private short gidLp = 0;

        // private Dane dane;
        public override void Init()
        {
            AddSubscription(true, 0, Events.OpenWindow, new TakeEventDelegate(OnOpenWindow));
            //AddSubscription(false, 0, Events.ResizeWindow, new TakeEventDelegate(Resize));
            // AddSubscription(false, 0, Events.Completed, new TakeEventDelegate(OnCloseWindow));
        }



        private bool OnOpenWindow(Procedures ProcId, int ControlId, Events Event)
        {

            // ClaWindow Parent = GetWindow();
            //Rectangle WndBnd = GetWindow().Bounds;
            //zakladkaTabWewnetrzne = GetWindow().AllChildren["?TabFS"];

            //Bufor= Parent.AllChildren["?lEkr_TransakcjaDoBufora"];
            //Akronim = Parent.AllChildren["?Knt:Akronim"];
            //AddSubscription(false, Bufor.Id, Events.Accepted, new TakeEventDelegate(OnCloseWindow));
            gidNumer = MagNag.MaN_GIDNumer;
            gidFirma = MagNag.MaN_GIDFirma;
            gidLp = MagNag.MaN_GIDLp;
            gidTyp = MagNag.MaN_GIDTyp;

            AddSubscription(false, 0, Events.Completed, new TakeEventDelegate(OnCloseWindow));

            //MessageBox.Show("GID Numer: " + gidNumer, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //listID = GetWindow().AllChildren["?Handlowe"].Id;      //tu sprawdzic

            //pobrane z pola w zakładce Raporty
            return true;
        }

        public bool Resize(Procedures ProcId, int ControlId, Events Event)
        {

            return true;
        }

        public bool OnCloseWindow(Procedures ProcId, int ControlId, Events Event)
        {

            try
            {
                string connection_string = "";
                //MessageBox.Show("GID numer: " + gidNumer, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                string transportZZ = "274";
                //string transportZZ = "268";
                string WMnaZZ = "272";
                //string WMnaZZ = "269";
                int XLWersja = 20221;
                XLLoginInfo_20221 xlLoginInfo20221 = new XLLoginInfo_20221();
                xlLoginInfo20221.Wersja = XLWersja;
                xlLoginInfo20221.UtworzWlasnaSesje = 0;
                xlLoginInfo20221.Winieta = -1;
                xlLoginInfo20221.TrybWsadowy = 1;
                xlLoginInfo20221.ProgramID = "WMnaZZ";
                int sesja = 0;
                int num3 = cdn_api.cdn_api.XLLogin(xlLoginInfo20221, ref sesja);
                if (num3 != 0)
                {
                    MessageBox.Show("Błąd logowania! (API " + num3.ToString() + ")");
                }
                else
                {
                    //MessageBox.Show("-------- Rozpoczęcie działania programu --------");
                    //Console.WriteLine();
                    XLPolaczenieInfo_20221 polaczenieInfo20221 = new XLPolaczenieInfo_20221();
                    polaczenieInfo20221.Wersja = XLWersja;
                    if (cdn_api.cdn_api.XLPolaczenie(polaczenieInfo20221) != 0)
                    {
                        MessageBox.Show("Błąd połączenia do bazy SQL!");
                    }
                    SqlConnection connection = new SqlConnection();
                    try
                    {
                        // connection = new SqlConnection(polaczenieInfo20221.ConnectString.Substring(polaczenieInfo20221.ConnectString.IndexOf("Data Source"), polaczenieInfo20221.ConnectString.Length - polaczenieInfo20221.ConnectString.IndexOf("Data Source")));
                        connection = new SqlConnection((polaczenieInfo20221.ConnectString.Substring(polaczenieInfo20221.ConnectString.IndexOf("Data Source"), polaczenieInfo20221.ConnectString.Length - polaczenieInfo20221.ConnectString.IndexOf("Data Source")) + "User ID = sabisup; Password=#0ut2510gro0ne.34").Replace("Integrated Security=SSPI", ""));
                        connection_string = (polaczenieInfo20221.ConnectString.Substring(polaczenieInfo20221.ConnectString.IndexOf("Data Source"), polaczenieInfo20221.ConnectString.Length - polaczenieInfo20221.ConnectString.IndexOf("Data Source")) + "User ID = sabisup; Password=#0ut2510gro0ne.34").Replace("Integrated Security=SSPI", "");

                        //MessageBox.Show(polaczenieInfo20220.ConnectString.Substring(polaczenieInfo20220.ConnectString.IndexOf("Data Source"), polaczenieInfo20220.ConnectString.Length - polaczenieInfo20220.ConnectString.IndexOf("Data Source")) + "; User ID = Wydruk; Password=S@msun6B@");
                    }
                    catch
                    {
                        MessageBox.Show("Błąd generowania połączenia " + polaczenieInfo20221.ConnectString);
                    }

                    connection.Open();
                    string czyAtrybutQuery = "SELECT Atr_Wartosc FROM cdn.Atrybuty join cdn.AtrybutyKLasy on AtK_Id = Atr_AtkId WHERE AtK_ID="+transportZZ+" and Atr_ObiTyp = " + gidTyp + " and Atr_ObiNumer = " + gidNumer;
                    string kntKod = ConnectToSQL(connection_string, czyAtrybutQuery);


                    string stanQuery = "select Man_Stan from cdn.MagNag where Man_GIDNumer="+gidNumer+" and Man_GIDTyp="+gidTyp;
                    string stan = ConnectToSQL(connection_string, stanQuery).Trim();


                    string numerMagQuery = "SELECT  cdn.NumerDokumentu(MAN_GIDTyp, MaN_SpeTyp, MaN_TrNTyp, MaN_TrNNumer, MaN_TrNRok, Man_TrNSeria, Man_TrNMiesiac) from cdn.MagNag where MaN_GIDTyp=" + gidTyp + " and Man_GIDNumer=" + gidNumer;
                    string numerMag = ConnectToSQL(connection_string, numerMagQuery);

                    string czyIstniejeQuery = "if exists(SELECT * from cdn.ZaNOpisy WHERE ZnO_Opis = '" + numerMag + "') SELECT 'TAK' else SELECT 'NIE'";
                    string czyIstnieje = ConnectToSQL(connection_string, czyIstniejeQuery).Trim();



                    //MessageBox.Show("Stan: " + stan);
                    if (kntKod!="" && stan=="5" && czyIstnieje=="NIE")
                    {
                        //MessageBox.Show("Istnieje atrybut");
                        string seriaQuery = "SELECT MaN_TrNSeria FROM cdn.MagNag where MaN_GIDTyp=" + gidTyp + " and Man_GIDNumer=" + gidNumer;
                        string seria = ConnectToSQL(connection_string, seriaQuery);

                        string magNumerQuery = "SELECT MaN_MagDNumer FROM cdn.MagNag where MaN_GIDTyp=" + gidTyp + " and Man_GIDNumer=" + gidNumer;
                        string magNumer = ConnectToSQL(connection_string, magNumerQuery );

                        //string numerMagQuery= "SELECT  cdn.NumerDokumentu(MAN_GIDTyp, MaN_SpeTyp, MaN_TrNTyp, MaN_TrNNumer, MaN_TrNRok, Man_TrNSeria, Man_TrNMiesiac) from cdn.MagNag where MaN_GIDTyp=" + gidTyp + " and Man_GIDNumer=" + gidNumer;
                        //string numerMag = ConnectToSQL(connection_string, numerMagQuery);

                        int dokId = 0;
                        XLDokumentZamNagInfo_20221 dokNag = new XLDokumentZamNagInfo_20221();
                        dokNag.GIDFirma = 0;
                        dokNag.GIDNumer = 0;
                        dokNag.GIDTyp = 0;
                        dokNag.GIDLp = 0;
                        dokNag.Akronim = kntKod;
                        dokNag.Wersja = XLWersja;
                        dokNag.Typ = 5;
                        dokNag.ZamSeria = seria == "" ? "<PUS>" : seria;
                        dokNag.MagNumer = Convert.ToInt32(magNumer);
                        dokNag.MagFirma = 1083914;
                        dokNag.MagTyp = 208;
                        dokNag.MagLp = 0;
                        dokNag.Opis = numerMag;
                        dokNag.Tryb = 1;
                        int blad=cdn_api.cdn_api.XLNowyDokumentZam(sesja, ref dokId, dokNag);
                        if(blad!=0)
                        {
                            MessageBox.Show("Błąd tworzenia dokumentu ZZ: " + blad, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            cdn_api.cdn_api.XLLogout(sesja);

                            connection.Close();
                            return false;
                        }

                        string towaryQuery = "SELECT Atr_ObiTyp, Atr_ObiFirma, Atr_ObiNumer , Atr_ObiLp FROM cdn.Atrybuty join cdn.AtrybutyKLasy on AtK_Id = Atr_AtkId WHERE AtK_Id = "+WMnaZZ+" and Atr_Wartosc = 'TAK'";
                        List<ListaDok> towary = SelectListDoki(towaryQuery, "cdn.Atrybuty", connection_string);

                        foreach(ListaDok towar in towary)
                        {
                            XLDokumentZamElemInfo_20221 poz = new XLDokumentZamElemInfo_20221();
                            poz.TwrFirma = towar.GIDFirma;
                            poz.TwrLp = towar.GIDLp;
                            poz.TwrNumer = towar.GIDNumer;
                            poz.TwrTyp = towar.GIDTyp;
                            poz.Wersja = XLWersja;
                            poz.Ilosc = "1";
                            poz.CenaOferowana = "0";
                            poz.CenaKatalogowa = "0";
                            //poz.Wartosc = "0";
                            poz.Rabat = "0";
                            poz.RabatReq = 1;
                            blad = cdn_api.cdn_api.XLDodajPozycjeZam(dokId, poz);
                            if (blad != 0)
                            {
                                MessageBox.Show("Błąd tworzenia pozycji ZZ "+towar.GIDNumer+" : " + blad, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                
                            }

                            XLDokumentZamElemModInfo_20221 pozMod = new XLDokumentZamElemModInfo_20221();
                            //pozMod.Cena = "0";
                            //pozMod.CenaP = "0";
                            pozMod.EleLp = poz.GIDLp;
                            //pozMod.FlagaVat = poz.FlagaVat;
                            pozMod.Ilosc = "1";
                            //pozMod.Rabat = 0;
                            pozMod.StawkaVAT = poz.StawkaVAT;
                            //pozMod.Wartosc = "0";
                            pozMod.Wersja = XLWersja;
                            pozMod.Waluta = poz.Waluta;
                            pozMod.Vat = poz.GrupaVAT;
                            blad = cdn_api.cdn_api.XLModyfikujPozycjeZam(dokId, pozMod);
                            if (blad != 0)
                            {
                                MessageBox.Show("Błąd modyfikacji pozycji ZZ " + towar.GIDNumer + " : " + blad, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                            }




                        }


                        XLZamkniecieDokumentuZamInfo_20221 zamNag = new XLZamkniecieDokumentuZamInfo_20221();
                        zamNag.Wersja = XLWersja;
                        zamNag.TrybZamkniecia = 0;
                        cdn_api.cdn_api.XLZamknijDokumentZam(dokId, zamNag);


                        string numerZrdQuery= "SELECT cdn.NumerDokumentu(TrN_GIDTyp, TrN_SpiTyp, TrN_TrNTyp, TrN_TrNNumer, TrN_TrNRok, TrN_TrNSeria, TrN_TrNMiesiac) FROM cdn.Magnag " +
                                                " join cdn.TraNag on MaN_ZrdNumer = TrN_GIDNumer and MaN_ZrdTyp = TrN_GIDTyp WHERE "+//TrN_GIDTyp = 2033 and " +
                                                " MaN_GIDTyp=" + gidTyp + " and Man_GIDNumer=" + gidNumer;
                        string numerZrd = ConnectToSQL(connection_string, numerZrdQuery);

                        if (numerZrd != "")
                        {
                           

                            string numerFSQuery = "SELECT TrN_GIDNumer FROM cdn.Magnag " +
                                                " join cdn.TraNag on MaN_ZrdNumer = TrN_GIDNumer and MaN_ZrdTyp = TrN_GIDTyp WHERE "+// TrN_GIDTyp = 2033 and" +
                                                " MaN_GIDTyp=" + gidTyp + " and Man_GIDNumer=" + gidNumer;
                            string numerFS = ConnectToSQL(connection_string, numerFSQuery);


                            string typFSQuery = "SELECT TrN_GIDTyp FROM cdn.Magnag " +
                                                " join cdn.TraNag on MaN_ZrdNumer = TrN_GIDNumer and MaN_ZrdTyp = TrN_GIDTyp WHERE " +// TrN_GIDTyp = 2033 and" +
                                                " MaN_GIDTyp=" + gidTyp + " and Man_GIDNumer=" + gidNumer;
                            string typFS = ConnectToSQL(connection_string, typFSQuery);
                            //MessageBox.Show("Numer zrd: " + numerZrd + "\nNumerFS:" + numerFS + "\nTyp FS :" + typFS);

                            string updateQuery = "update a set a.Atr_Wartosc='" + numerZrd + "' , atr_AtrTyp= "+typFS+" ,atr_AtrFirma=" + dokNag.GIDFirma + ", atr_atrNumer=" + numerFS + " from cdn.Atrybuty as a join   cdn.AtrybutyKlasy on Atk_id=Atr_AtkId " +
                                                " WHERE AtK_Nazwa = 'Dokument sprzedaży' and Atr_ObiNumer = " + dokNag.GIDNumer + " and atr_ObiTyp = " + dokNag.GIDTyp;
                            Update(updateQuery, connection_string);
                            

                        }

                         string numerZamQuery = "SELECT  Replace(cdn.NumerDokumentu(Zan_GIDTyp, ZaN_GIDTyp, ZaN_ZamTyp, Zan_ZamNumer, ZaN_ZamRok, ZaN_ZamSeria, ZaN_ZamMiesiac), 'Zam', 'ZZ') from cdn.ZamNag " +
                                           " where ZaN_GIDTyp=" +dokNag.GIDTyp +" and Zan_GIDNumer=" + dokNag.GIDNumer;
                         string numerZam = ConnectToSQL(connection_string, numerZamQuery);

                        /*  string atrKlasaQuery = "SELECT Atk_NAzwa from cdn.AtrybutyKlasy where Atk_Id=279";
                         string atrKlasa = ConnectToSQL(connection_string, atrKlasaQuery);

                         XLAtrybutInfo_20221 atr = new XLAtrybutInfo_20221();
                         atr.Wersja = XLWersja;
                         atr.GIDFirma = Convert.ToInt32(gidFirma);
                         atr.GIDLp = 0;
                         atr.GIDNumer = Convert.ToInt32(gidNumer);
                         atr.GIDTyp = Convert.ToInt32(gidTyp);
                         atr.GIDSubLp = 0;
                         atr.Klasa = atrKlasa;
                         atr.Wartosc = numerZam;
                         int blad2 = cdn_api.cdn_api.XLDodajAtrybut(sesja, atr);
                         if (blad2 != 0)
                         {
                             MessageBox.Show("Błąd dodawania atrybutu " + atrKlasa + " : " + blad2, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                         }*/


                        string updateAtr = "UPDATE cdn.Atrybuty set atr_Wartosc='" + numerZam + "' where Atr_AtkId=279 and Atr_ObiNumer=" + gidNumer + " and Atr_ObiTyp=" + gidTyp;
                        Update(updateAtr, connection_string);







                        // MessageBox.Show("Wygenerowano dokument: " + numerZam, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    }



                    cdn_api.cdn_api.XLLogout(sesja);

                    


                    connection.Close();
                }
                return true;
            }
            catch (Exception e)
            {
                MessageBox.Show("Błąd funkcji onCloseWindow: " + e.Message, "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

        }

        public string ConnectToSQL(string connection_string, string query)
        {
            string str = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(query, connection);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Query");
                    if (dataSet.Tables.Count == 0)
                    {
                        return "";
                    }
                    foreach (DataRow row in (InternalDataCollectionBase)dataSet.Tables[0].Rows)
                    {
                        str = row[0].ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                str = str + "Bład SQL:" + ex.ToString() + "\n";
            }

            return str;
        }

        public string Update(string sql, string connection_string)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    connection.Open();
                    adapter.UpdateCommand = new SqlCommand(sql, connection);
                    adapter.UpdateCommand.ExecuteNonQuery();
                    //MessageBox.Show("Row updated !! ");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Komunikat", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return sql;
        }

        public static List<ListaDok> SelectListDoki(string query, string srcTableName, string connection_string)
        {
            List<ListaDok> str = new List<ListaDok>();
            string sttr = "";
            try
            {
                using (SqlConnection connection = new SqlConnection(connection_string))
                {
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = new SqlCommand(query, connection);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, srcTableName);
                    foreach (DataRow row in (InternalDataCollectionBase)dataSet.Tables[0].Rows)
                    {

                        str.Add(new ListaDok()
                        {
                            GIDTyp = Convert.ToInt16(row[0].ToString()),
                            GIDFirma = Convert.ToInt32(row[1].ToString()),
                            GIDNumer = Convert.ToInt32(row[2].ToString()),
                            GIDLp = Convert.ToInt16(row[3].ToString()),
                        });



                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(sttr = sttr + "Bład SQL:" + ex.ToString() + "\n");
            }
            return str;
        }


        public override void Cleanup()
        {
            //throw new NotImplementedException();
        }
    }
}
