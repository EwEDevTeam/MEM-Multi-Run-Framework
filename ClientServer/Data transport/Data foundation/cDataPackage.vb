' ===============================================================================
' This file is part of the Marine Ecosystem Model multi-run framework prototype, 
' developed for the PhD thesis of Jeroen Steenbeek (2020-2023) in Marine Sciences 
' at the Polytechnical University of Catalunya.
'
' The MEM run framework is distributed in the hope that it will be useful, but 
' WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or 
' FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more 
' details.
'
' You should have received a copy of the GNU General Public License along with EwE.
' If not, see <http:'www.gnu.org/licenses/gpl-2.0.html>. 
'
' Copyright 2020- 
'    Ecopath International Initiative, Barcelona, Spain
' ==============================================================================='

#Region " Imports "

Option Strict On
Imports System.Runtime.Serialization

#End Region ' Imports

''' ===========================================================================
''' <summary>
''' Base class for sending a <see cref="cSerializableObject">data package</see>
''' across a <see cref="cDataExchangeProtocol"/>, maintaining properties related
''' to network traffic.
''' </summary>
''' ===========================================================================
<Serializable()>
Public Class cDataPackage
    Inherits cSerializableObject

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sequence">Sequential number of a data package</param>
    ''' <param name="data">Data that is being transported</param>
    ''' <param name="sender">Sender ID</param>
    ''' <param name="recipient">Recipient ID, or zero if the datapackage has no 
    ''' particular target (e.g. is broadcasted data)</param>
    ''' <param name="sequenceReply">Sequence of preceding message, if any.</param>
    ''' -----------------------------------------------------------------------
    Public Sub New(sequence As ULong, data As cSerializableObject, sender As Int32, recipient As Int32, Optional sequenceReply As ULong = 0)
        Me.Sequence = sequence
        Me.ReplyToSequence = sequenceReply
        Me.Data = data
        Me.Sender = sender
        Me.Recipient = recipient
        Me.TimeUTM = Date.UtcNow
    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the sequential number of the message. Each framework node keeps an 
    ''' internal message counter. For now, no effort is made to wrap around the 
    ''' value limit of the Long value type. Yeah, that's pretty bad.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Sequence As ULong

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the <see cref="Sequence"/> of the message that this message replies to.
    ''' If this isn't a reply, this value will contain 0.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property ReplyToSequence As ULong

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the Transported data.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Data As cSerializableObject

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the ID of the sending node.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Sender As Int32

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the ID of the recepient node.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property Recipient As Int32

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get the global UTM time stamp of the package.
    ''' </summary>
    ''' -----------------------------------------------------------------------
    Public ReadOnly Property TimeUTM As DateTime

    Protected Sub New(info As SerializationInfo, context As StreamingContext)
        MyBase.New(info, context)
        Try
            Me.TimeUTM = info.GetDateTime("timeutm")
            Me.Sequence = info.GetUInt64("seq")
            Me.ReplyToSequence = info.GetUInt64("seqreply")
            Me.Sender = info.GetInt32("from")
            Me.Recipient = info.GetInt32("to")
            Me.Data = CType(info.GetValue("data", GetType(cSerializableObject)), cSerializableObject)
        Catch ex As Exception
            Debug.Assert(False, String.Format("Exception '{0}' while deserializing cFileDataPackage", ex.Message))
        End Try
    End Sub

    Protected Overrides Sub GetObjectData(info As System.Runtime.Serialization.SerializationInfo, context As System.Runtime.Serialization.StreamingContext)
        MyBase.GetObjectData(info, context)
        info.AddValue("seq", Me.Sequence, GetType(ULong))
        info.AddValue("seqreply", Me.ReplyToSequence, GetType(ULong))
        info.AddValue("from", Me.Sender, GetType(Int32))
        info.AddValue("to", Me.Recipient, GetType(Int32))
        info.AddValue("data", Me.Data, GetType(Int32))
        info.AddValue("timeutm", Me.TimeUTM, GetType(DateTime))
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("#{0} {1}>{2} {3}", Me.Sequence, Me.Sender, Me.Recipient, Me.Data)
    End Function

End Class
