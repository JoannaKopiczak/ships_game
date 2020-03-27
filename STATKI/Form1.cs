using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace STATKI
{
    public partial class Form1 : Form
    {
        List<Button> player1Position;
        List<Button> player2Position;
        Random rand = new Random();
        int totalShipsPlayer1 = 3;

        int rounds = 10;
        int totalScorePlayer1 = 0;
        int totalScorePlayer2 = 0;
        public byte[] _buffer;
        public byte[] _buffer2;
        private Socket _clientSocket;
        public int publicznyindeks;
        string text;
        string text2 = "A5";


        public Form1()
        {
            InitializeComponent();
            loadbuttons();
            attackButton.Enabled = false;
            player2LocationList.Text = null;
        }

        private void loadbuttons()
        {
            player1Position = new List<Button> { w1, w2, w3, w4, x1, x2, x3, x4, y1, y2, y3, y4, z1, z2, z3, z4 };
            player2Position = new List<Button> { a1, a2, a3, a4, b1, b2, b3, b4, c1, c2, c3, c4, d1, d2, d3, d4 };

            for (int i = 0; i <= player2Position.Count - 1; i++)
            {

                player2Position[i].Tag = null;
                player2LocationList.Items.Add(player2Position[i].Text);
                string liczba = Convert.ToString(player2Position.Count);

            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse("192.168.1.105"), 8001), new AsyncCallback(ConnectCallback), null);
                helpText.Text = "2) Wybierz  trzy pola na których ustawisz statki. ";

            }
            catch (SocketException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        

        private void Player1PicksPosition(object sender, EventArgs e)
        {
            if (totalShipsPlayer1 > 0)
            {
                var button = (Button)sender;
                button.Enabled = false;
                button.Tag = "player1ship";
                button.BackColor = System.Drawing.Color.Maroon;
                totalShipsPlayer1--;
            }


            if (totalShipsPlayer1 == 0)
            {
                attackButton.Enabled = true;
                attackButton.BackColor = System.Drawing.Color.Maroon;
                helpText.Top = 70;
                helpText.Left = 70;
                helpText.Text = "3) Teraz wybierz pozycje ataku.";

              
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {

                int bytesRead = _clientSocket.EndReceive(ar);
                text = Encoding.ASCII.GetString(_buffer);
                while (bytesRead == 0 && text == text2)
                { return; }

                Array.Resize(ref _buffer, bytesRead);
               
                text2 = Encoding.ASCII.GetString(_buffer);
                Array.Resize(ref _buffer, _clientSocket.ReceiveBufferSize);

                string[] w = text2.Split(' ');
                string first = w[0];
                string second = w[1];
                if (first == "atakklienta")
                {
                    rounds--;
                    
                    int index = 0;
                    while ((player2Position[index].Text != second) && ((player2Position[index].Text + "p") != second)) { index++; }

                    if ((player2Position[index].Text + "p") == second)
                    {

                        player2Position[index].Invoke(new Action(() => player2Position[index].BackgroundImage = Properties.Resources.missIcon));
                        player2Position[index].Invoke(new Action(() => player2Position[index].BackColor = System.Drawing.Color.DarkBlue));
                        player2Position[index].Invoke(new Action(() => player2Position[index].Enabled = false));

                        helpText.Invoke(new Action(() => helpText.Text = "Teraz kolej przeciwnika."));

                        roundsText.Invoke(new Action(() => roundsText.Text = "Runda " + rounds));
                       

                    }
                    else if (player2Position[index].Text == second)
                    {
                        player2Position[index].Invoke(new Action(() => player2Position[index].BackgroundImage = Properties.Resources.fireIcon));
                        player2Position[index].Invoke(new Action(() => player2Position[index].BackColor = System.Drawing.Color.DarkBlue));
                        player2Position[index].Invoke(new Action(() => player2Position[index].Enabled = false));
                        helpText.Invoke(new Action(() => helpText.Text = "Teraz kolej przeciwnika."));
                        roundsText.Invoke(new Action(() => roundsText.Text = "Runda " + rounds));
                        totalScorePlayer1++;
                        player1Score.Invoke(new Action(() => player1Score.Text = " " + totalScorePlayer1));
                        
                    }
                    if (rounds < 1 || totalScorePlayer1 > 2 || totalScorePlayer2 > 2)
                    {
                        if (totalScorePlayer1 > totalScorePlayer2)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("WYGRAŁEŚ !!!");
                        }

                        if (totalScorePlayer1 == totalScorePlayer2)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("REMIS!");
                        }

                        if (totalScorePlayer2 > totalScorePlayer1)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("PRZEGRAŁEŚ :(((");
                        }
                    }

                   
                    _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
                   
                }
                else if (first == "atakserwera")
                {
                    rounds--;
                    int index = 0;
                    while (player1Position[index].Text != second) { index++; }
                    
                    if ((string)player1Position[index].Tag == "player1ship")
                    {
                        byte[] buffer = Encoding.ASCII.GetBytes("atakserwera " + player1Position[index].Text);
                        _clientSocket.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallback), _clientSocket);
                        totalScorePlayer2++;
                        player2Score.Invoke(new Action(() => player2Score.Text = " " + totalScorePlayer2));
                        roundsText.Invoke(new Action(() => roundsText.Text = "Runda " + rounds));
                        player2Moves.Invoke(new Action(() => player2Moves.Text = player1Position[index].Text));
                        helpText.Invoke(new Action(() => helpText.Text = "Strzelaj!"));
                    }
                    else
                    {
                        byte[] buffer = Encoding.ASCII.GetBytes("atakserwera " + player1Position[index].Text + "p");
                        _clientSocket.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallback), _clientSocket);
                        roundsText.Invoke(new Action(() => roundsText.Text = "Runda " + rounds));
                        player2Moves.Invoke(new Action(() => player2Moves.Text = player1Position[index].Text));
                        helpText.Invoke(new Action(() => helpText.Text = "Strzelaj!"));
                    }
                    if (rounds < 1 || totalScorePlayer1 > 2 || totalScorePlayer2 > 2)
                    {
                        if (totalScorePlayer1 > totalScorePlayer2)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("WYGRAŁEŚ !!!");

                        }

                        if (totalScorePlayer1 == totalScorePlayer2)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("REMIS!");

                        }

                        if (totalScorePlayer2 > totalScorePlayer1)
                        {
                            helpText.Invoke(new Action(() => helpText.Text = "Koniec gry!"));
                            MessageBox.Show("PRZEGRAŁEŚ :(((");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        private void EndReceive(IAsyncResult ar)
        {
            _clientSocket.EndReceive(ar);
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _clientSocket.EndConnect(ar);
                button1.Enabled = true;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket _clientsocket = (Socket)ar.AsyncState;
                _clientSocket.EndSend(ar);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AttackPlayer2Position(object sender, EventArgs e)
        {
            if (player2LocationList.Text != " ")
            {
                var attackPos = player2LocationList.Text;
                attackPos = attackPos.ToLower();
                int index = player2Position.FindIndex(a => a.Name == attackPos);
                string bla = Convert.ToString(index);
               

                if (player2Position[index].Enabled && rounds > 0)
                {

                    roundsText.Text = "Runda " + rounds;


                    byte[] buffer = Encoding.ASCII.GetBytes("atakklienta " + player2Position[index].Text);

                    _clientSocket.BeginSend(buffer, 0, buffer.Length, 0, new AsyncCallback(SendCallback), _clientSocket);
                    
                    _buffer = new byte[20];
                    
                    _clientSocket.BeginReceive(_buffer, 0, _buffer.Length, 0, new AsyncCallback(ReceiveCallback), _clientSocket);


                }

            }
            else
            {
                MessageBox.Show("Wybierz pozycje z listy!");
            }

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void helpText_Click(object sender, EventArgs e)
        {

        }
    }
}
