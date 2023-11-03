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

#End Region

''' ===========================================================================
''' <summary>
''' Foundation class for tasks that run in-process. Please ensure that the 
''' implementing instances adhere to thread-safeness and thread marshalling in 
''' their implementation of <see cref="cTaskRunner.RunTask()"/>
''' </summary>
''' ===========================================================================
Public MustInherit Class cEmbeddedTaskRunner
    Inherits cTaskRunner

    Public Sub New(parms As String)
        MyBase.New(parms)
    End Sub

    Public Overrides Function ToString() As String
        Return Me.GetType().ToString()
    End Function

#If DEBUG Then
    Protected Overrides Sub DumpConfiguration()
        Debug.WriteLine("- Class {0}", Me.GetType.ToString())
        Debug.WriteLine("- Parms {0}", Me.Parameters)
    End Sub
#End If

End Class
