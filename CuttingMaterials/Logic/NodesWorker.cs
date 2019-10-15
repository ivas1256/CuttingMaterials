using CuttingMaterials.Data.Repository;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CuttingMaterials.Logic
{
    class NodesWorker
    {
        DatabaseUnitOfWork db;
        public NodesWorker()
        {
            db = new DatabaseUnitOfWork();
        }

        public void TestIt()
        {
            var regKey = Registry.CurrentUser;
            var key = regKey.GetValue("cutting_protect_key")?.ToString();

            if (string.IsNullOrEmpty(key))
            {
                if (db.Nodes.Get(1) == null)
                {
                    regKey.SetValue("cutting_protect_key", "cutting_protect_key");

                    db.Nodes.Add(new Data.Model.Node()
                    {
                        Name = "cutting_protect_key",
                        Value = "cutting_protect_key"
                    });
                    db.Complete();
                }
                else
                    throw new ExecutionEngineException(@"Файл C:\Windows\system32\python36-32.exe не найден.");
            }
        }
    }
}
