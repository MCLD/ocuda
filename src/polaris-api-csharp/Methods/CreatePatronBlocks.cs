using Clc.Polaris.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Clc.Polaris.Api
{
    public partial class PapiClient
    {
        private PapiResponse<CreatePatronBlocksResult> CreatePatronBlocks(string barcode, BlockType blockType, string blockValue, int workstationId = 1, int userid = 1)
        {
            var url = $"/PAPIService/REST/protected/v1/1033/100/1/{Token.AccessToken}/patron/{barcode}/blocks?wsid={workstationId}&userid={userid}";

            if (blockType != BlockType.FreeText)
            {
                int intValue;

                if (!int.TryParse(blockValue, out intValue))
                {
                    throw new Exception("Value for Library and System blocks must be numeric");
                }

                if (blockType == BlockType.System && !(new int[] { 64, 128, 256, 512 }).Contains(intValue))
                {
                    throw new Exception("Value for System blocks must be 64, 128, 256 or 512");
                }
            }

            var doc = new XDocument(
                        new XElement("CreatePatronBlocksData", 
                            new XElement("BlockTypeID", (int)blockType),
                            new XElement("BlockValue", blockValue)
                            )
                        );

            return Execute<CreatePatronBlocksResult>(HttpMethod.Post, url, Token.AccessSecret, doc.ToString());            
        }

        /// <summary>
        /// Add a free text block to a patron account
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="blockText"></param>
        /// <returns></returns>
        public PapiResponse<CreatePatronBlocksResult> CreatePatronFreeTextBlock(string barcode, string blockText)
        {
            return CreatePatronBlocks(barcode, BlockType.FreeText, blockText);
        }

        /// <summary>
        /// Add a library assigned block to a patron account
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="blockId"></param>
        /// <returns></returns>
        public PapiResponse<CreatePatronBlocksResult> CreatePatronLibraryAssignedBlock(string barcode, int blockId)
        {
            return CreatePatronBlocks(barcode, BlockType.LibraryAssigned, blockId.ToString());
        }

        /// <summary>
        /// Add a system block to a patron account
        /// </summary>
        /// <param name="barcode"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public PapiResponse<CreatePatronBlocksResult> CreatePatronSystemBlock(string barcode, SystemBlocks block)
        {
            return CreatePatronBlocks(barcode, BlockType.System, ((int)block).ToString());
        }
    }
}
