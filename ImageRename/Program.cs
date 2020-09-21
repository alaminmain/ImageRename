using Dapper;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using Dapper.Oracle;

namespace ImageRename

{
    class Program
    {
        static void RenameImage()
        {
            //Get Image Name from Database
            string conString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=****)(PORT=1521)))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCL)));User Id=*****;Password=****";
            List<EMP_IMG_INFO> obj = new List<EMP_IMG_INFO>();
            try
            {
                using (var con = new OracleConnection(conString))
                {
                    con.Open();
                    //var p = new OracleDynamicParameters();
                    //p.BindByName = true;
                    //p.Add(name: ":BALAN_DATE", dbType: OracleMappingType.Varchar2, direction: ParameterDirection.Input, value: balanceDate);

                    //p.Add(name: ":cursor_balance_data", dbType: OracleMappingType.RefCursor, direction: ParameterDirection.Output);
                    string sql = @"SELECT  EMP_ID, IMG_URL, INS_UPD_DATE, OP_NAME FROM PMIS.EMP_IMG_INFO";

                    obj = con.Query<EMP_IMG_INFO>
                        (sql, 
                        commandType: CommandType.Text).ToList();
                    int counter = 0;
                    con.Close();
                    //Rename image name by user_login_id in file
                    foreach (var item in obj)
                    {
                        var getfilename = item.IMG_URL.Substring(18);
                        string dirpath = Directory.GetCurrentDirectory();
                        string fileLocation=Path.Combine(Directory.GetCurrentDirectory(), getfilename);
                        string filenamewithoutExtention = Path.GetFileNameWithoutExtension(fileLocation);
                        string extension = Path.GetExtension(fileLocation);
                        string filenameforDB = "~/Employee_Images/" + item.EMP_ID+extension;
                        
                        if (File.Exists(fileLocation))
                        {
                            if (filenamewithoutExtention != item.EMP_ID)
                            {
                                //Check if file Exist or not. If Exist then Delete the File
                                if (File.Exists(item.EMP_ID + extension))
                                    System.IO.File.Delete(item.EMP_ID + extension);

                                System.IO.File.Move(getfilename, item.EMP_ID + extension);
                                System.IO.File.Delete(fileLocation);
                                counter++;
                            }
                        }

                        //Update Image Name in database

                        con.Open();

                        string update = @"update PMIS.EMP_IMG_INFO set IMG_URL=:newImgUrl, INS_UPD_DATE=sysdate, OP_NAME='20130006' where emp_id=:emp_id";

                        var parameters = new OracleDynamicParameters();

                        parameters.BindByName = true;
                        parameters.Add(name: ":emp_id", dbType: OracleMappingType.Int32, direction: ParameterDirection.Input, value: item.EMP_ID);
                        var i = con.Execute(update, parameters);
                        con.Close();

                    }
                    Console.WriteLine(counter);
                }
                Console.WriteLine("Successfull");
            }
            catch (Exception ex)
            {
                // return null;
                Console.WriteLine("Successfull");
            }
            //

            

        }
        static void Main(string[] args)
        {
            RenameImage();
            Console.ReadKey();
            
            //Console.WriteLine("Hello World!");
        }
    }
}
