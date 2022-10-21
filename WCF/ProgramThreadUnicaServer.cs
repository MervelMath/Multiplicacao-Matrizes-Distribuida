using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;

namespace WCF.Server
{
    public class ProgramThreadUnicaServer
    {
        [ServiceContract]
        public interface IMessageService {
            [OperationContract]
            byte[] MultiplicaMatrizes(byte[] matrizABytes, byte[] matrizBBytes, int range);

        }

        [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
        public class MessageService : IMessageService
        {
            public byte[] MultiplicaMatrizes(byte[] matrizABytes, byte[] matrizBBytes, int range)
            {
                double[,] matrizA = null, matrizB = null;

                using (MemoryStream ms = new MemoryStream(matrizABytes))
                {
                    IFormatter br = new BinaryFormatter();
                    matrizA = (double[,])br.Deserialize(ms);

                    //Console.WriteLine(matrizA.ToString());
                }

                using (MemoryStream ms = new MemoryStream(matrizABytes))
                {
                    IFormatter br = new BinaryFormatter();
                    matrizB = (double[,])br.Deserialize(ms);

                   // Console.WriteLine(matrizB.ToString());
                }

                IFormatter formatter = new BinaryFormatter();
                var msRetorno = new MemoryStream();

                formatter.Serialize(msRetorno, MultiplicaMatrizesDinamicamente(matrizA, matrizB, range));
                var matrizC1Bytes = msRetorno.ToArray();

                return matrizC1Bytes;
            }
        }
        static void Main(string[] args)
        {
            var uris = new Uri[1];
            string address = "net.tcp://localhost:6565/MessageService";
            uris[0] = new Uri(address);
            IMessageService message = new MessageService();            
            ServiceHost host = new ServiceHost(message, uris);
            var binding = new NetTcpBinding(SecurityMode.None);
            binding.Security.Mode = SecurityMode.Transport;
            binding.Security.Transport.ClientCredentialType = TcpClientCredentialType.Windows;
            binding.Security.Transport.ProtectionLevel = System.Net.Security.ProtectionLevel.EncryptAndSign;
            binding.MaxReceivedMessageSize = 1000000000;
            binding.OpenTimeout = TimeSpan.FromMinutes(40);
            binding.SendTimeout = TimeSpan.FromMinutes(40);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(40);
            host.AddServiceEndpoint(typeof(IMessageService), binding, "");
            host.Opened += Host_Opened;
            host.Open();
            Console.ReadLine();
        }

        private static void Host_Opened(object sender, EventArgs e)
        {
            Console.WriteLine("Message service started");
        }

        public static double[,] MultiplicaMatrizesDinamicamente(double[,] matrizA, double[,] matrizB, int range)
        {
            double acumula = 0;
            double[,] matriz = new double[range, range];

            //cada iteração representa uma linha da matriz A
            for (int linha = 0; linha < range; linha++)
            {
                

                //em cada linha de A, itera nas colunas de B
                for (int coluna = 0; coluna < range; coluna++)
                {
                    //itera, ao mesmo tempo, entre os elementos da linha de A e da coluna de B
                    for (int i = 0; i < range; i++)
                    {
                        //acumula representa os valores que estávamos reservando
                        acumula = acumula + matrizA[linha, i] * matrizB[i, coluna];
                    }
                    //quando a execução está aqui, já se tem mais um elemento da matriz AB
                    matriz[linha, coluna] = acumula;

                    //a variável então é zerada para que possa referenciar um novo elemento de AB
                    acumula = 0;
                }
                Console.Clear();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Matrizes de 0 à {range} multiplicadas com sucesso.");
            Console.ResetColor();

            return matriz;
        }
    }
}
