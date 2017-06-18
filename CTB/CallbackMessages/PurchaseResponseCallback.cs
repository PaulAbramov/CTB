/*

___    ___  ______   ________    __       ______
\  \  /  / |   ___| |__    __|  /  \     |   ___|
 \  \/  /  |  |___     |  |    / /\ \    |  |__
  |    |   |   ___|    |  |   /  __  \    \__  \
 /	/\  \  |  |___     |  |  /  /  \  \   ___|  |
/__/  \__\ |______|    |__| /__/    \__\ |______|

Written by Paul "Xetas" Abramov


*/

using System.Collections.Generic;
using System.IO;
using System.Net;
using SteamKit2;
using SteamKit2.Internal;

namespace CTB.CallbackMessages
{
    /// <summary>
    /// Custom PurchaseResponseCallback which will be sent to steam and returned with some filled informations
    /// Has to inherit from "CallbackMsg" to use it as a Callback with steam
    /// Using it as a callback, we want to have a Result, PurchaseResultDetail, the items we wanted to purchase/activate
    /// </summary>
    public class PurchaseResponseCallback : CallbackMsg
    {
        public Dictionary<uint, string> m_Items = new Dictionary<uint, string>();


        public EPurchaseResultDetail m_PurchaseResultDetail;
        public EResult m_Result;

        /// <summary>
        /// Constructor
        /// 
        /// First we want to pass a jobID so we can identify the callback if we are going to receive it as an answer from steam
        /// From the returned "_clientPurchaseMessage" we want to get the purchase result and also the general result
        /// 
        /// To get Informations about all the games we wanted to activate and if they were successfully activated and so on,
        /// We will have to readout the bytearray we got from steam inside of "purchase_receipt_info"
        /// This contains a KeyValueCollection converted to a bytearray, so we want to reconvert it, so we can use it
        /// 
        /// Inside the KeyValueCollection we got, we want to get the KeyValuePair or a list of the KeyValuePairs "lineitems"
        /// This holds the games and the appIDs we wanted to activate
        /// For each of this objects we want to get the PackageID, if it is not available we want to get the AppID for this game
        /// Also we do want to get the games name and add it to the list at the position of the packageID
        /// The gameName has to be HTMLDecoded
        /// </summary>
        /// <param name="_jobID"></param>
        /// <param name="_clientPurchaseMessage"></param>
        public PurchaseResponseCallback(JobID _jobID, CMsgClientPurchaseResponse _clientPurchaseMessage)
        {
            JobID = _jobID;
            m_PurchaseResultDetail = (EPurchaseResultDetail) _clientPurchaseMessage.purchase_result_details;
            m_Result = (EResult) _clientPurchaseMessage.eresult;

            KeyValue receiptInfo = new KeyValue();
            using (MemoryStream memoryStream = new MemoryStream(_clientPurchaseMessage.purchase_receipt_info))
            {
                if(!receiptInfo.TryReadAsBinary(memoryStream))
                {
                    return;
                }
            }

            List<KeyValue> lineItems = receiptInfo["lineitems"].Children;

            foreach(KeyValue lineItem in lineItems)
            {
                uint packageID = lineItem["PackageID"].AsUnsignedInteger();
                if (packageID == 0)
                {
                    packageID = lineItem["ItemAppID"].AsUnsignedInteger();
                    if (packageID == 0)
                    {
                        return;
                    }
                }
                
                string gameName = lineItem["ItemDescription"].Value;
                if (string.IsNullOrEmpty(gameName))
                {
                    return;
                }
                
                gameName = WebUtility.HtmlDecode(gameName);
                m_Items[packageID] = gameName;
            }
        }
    }
}
