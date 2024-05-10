using Melange;

int port = Environment.GetEnvironmentVariable("PORT") != null ? Convert.ToInt32(Environment.GetEnvironmentVariable("PORT")) : 8880;
MelangeNode node = new MelangeNode(port);
node.Start();
