using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IzendaService
{
	public class CustomWebClient : WebClient
	{
		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest request = (WebRequest)base.GetWebRequest(address);
			request.PreAuthenticate = true;
			return request;
		}
	}
}
