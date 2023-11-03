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
Imports System.Reflection

#End Region

Public Class cTypeUtils

    Public Shared Function TypeToString(t As Type) As String

        Return t.FullName()

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Helper method, locates the originating type from a type string.
    ''' </summary>
    ''' <param name="name">The type name to locate the originating type
    ''' for.</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' The counterpart of this method, <see cref="TypeToString"/>,
    ''' can be used to create the string for a type.
    ''' </remarks>
    ''' -------------------------------------------------------------------
    Public Shared Function StringToType(name As String) As Type

        ' Split assembly short name from type name
        Dim bits As String() = name.Split("."c)
        Dim ass As Assembly = Nothing

        For Each ass In AppDomain.CurrentDomain.GetAssemblies
            If String.Compare(ass.GetName.Name, bits(0), True) = 0 Then
                Try
                    Return ass.GetType(name, False, True)
                Catch ex As Exception

                End Try
            End If
        Next
        Return Nothing

    End Function

    ''' -------------------------------------------------------------------
    ''' <summary>
    ''' Get an array of a all public, non-abstract derived types.
    ''' </summary>
    ''' <param name="t"></param>
    ''' <returns></returns>
    ''' <remarks>Yeah, this could be rewritten is some powerful but illegible 
    ''' Lync expression too. Bah.</remarks>
    ''' -------------------------------------------------------------------
    Public Shared Function FindType(Of T)() As T()

        Dim lTypes As New List(Of T)
        Dim target As Type = GetType(T)
        For Each asm As Assembly In AppDomain.CurrentDomain.GetAssemblies()
            For Each test As Type In asm.GetTypes()
                If target.IsAssignableFrom(test) And (Not test.IsAbstract()) And (test.IsPublic) Then
                    Try
                        lTypes.Add(DirectCast(Activator.CreateInstance(test), T))
                    Catch ex As Exception

                    End Try
                End If
            Next
        Next
        Return lTypes.ToArray()

    End Function


End Class

