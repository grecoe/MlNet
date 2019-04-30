//
// Copyright  Microsoft Corporation ("Microsoft").
//
// Microsoft grants you the right to use this software in accordance with your subscription agreement, if any, to use software 
// provided for use with Microsoft Azure ("Subscription Agreement").  All software is licensed, not sold.  
// 
// If you do not have a Subscription Agreement, or at your option if you so choose, Microsoft grants you a nonexclusive, perpetual, 
// royalty-free right to use and modify this software solely for your internal business purposes in connection with Microsoft Azure 
// and other Microsoft products, including but not limited to, Microsoft R Open, Microsoft R Server, and Microsoft SQL Server.  
// 
// Unless otherwise stated in your Subscription Agreement, the following applies.  THIS SOFTWARE IS PROVIDED "AS IS" WITHOUT 
// WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL MICROSOFT OR ITS LICENSORS BE LIABLE 
// FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED 
// TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) 
// HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THE SAMPLE CODE, EVEN IF ADVISED OF THE
// POSSIBILITY OF SUCH DAMAGE.
//

using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.Text;

namespace MLNetWrapper.Interfaces
{
    /// <summary>
    /// Represents how the model is saved and loaded from the desired storage 
    /// location.
    /// </summary>
    public interface IModelPersistence
    {
        /// <summary>
        /// Instance of IModelLocation identifying where the model lives.
        /// </summary>
        IModelLocation ModelLocation { get; }

        /// <summary>
        /// Used to set the internal model file when first created or retrained 
        /// with an instance of IModellingBase so that the correct model is 
        /// persisted.
        /// </summary>
        /// <param name="model"></param>
        void SetModel(ITransformer model);

        /// <summary>
        /// Loads the model from the desired location.
        /// </summary>
        /// <param name="context">An instance of MLContext</param>
        /// <returns>An instance of the model.</returns>
        ITransformer LoadModel(MLContext context);

        /// <summary>
        /// Saves the internal model to the desired location.
        /// </summary>
        /// <param name="context">An instance of MLContext</param>
        /// <returns>True if persisted.</returns>
        bool SaveModel(MLContext context);
    }
}
