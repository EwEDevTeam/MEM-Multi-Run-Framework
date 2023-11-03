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

#End Region ' Imports

''' =============================================================================
''' <summary>
''' Class to manage aliases to folders on a local system. Aliase names are not 
''' case-sensitive.
''' </summary>
''' =============================================================================
Public Class cFolderAliases
    Inherits cAliases

    Public Const ALIAS_JOBNAME As String = "JobName"
    Public Const ALIAS_TASKNAME As String = "TaskName"
    Public Const ALIAS_MAXTHREADS As String = "MaxThreads"

    Public Sub New()

        MyBase.New()

        Me.ReserveAlias(ALIAS_JOBNAME)
        Me.ReserveAlias(ALIAS_TASKNAME)
        Me.ReserveAlias(ALIAS_MAXTHREADS)

    End Sub

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Expand a path, replacing all alias fields with their registered values.
    ''' </summary>
    ''' <param name="pin">The path to expand.</param>
    ''' <returns>The expanded path.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Expand(pin As String) As String
        Return Me.Expand(pin, "", "", -1)
    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Expand a path, replacing all alias fields with their registered values,
    ''' and also expanding system aliases <see cref="ALIAS_JOBNAME"/> and 
    ''' <see cref="ALIAS_TASKNAME"/> with the names of a provided job and task. 
    ''' Optionally, a thread count cap <see cref="ALIAS_MAXTHREADS"/> can be provided.
    ''' </summary>
    ''' <param name="pin">The path to expand.</param>
    ''' <param name="jobName">The name of the job to expand into the path.</param>
    ''' <param name="task">The name of the task to expand into the path.</param>
    ''' <returns>The expanded path.</returns>
    ''' -----------------------------------------------------------------------
    Public Function Expand(pin As String, jobName As String, taskName As String, iMaxThreads As Integer) As String

        Dim pout As String = pin

        For Each a As String In Me.Aliases
            pout = pout.Replace(Me.AliasStarter & a & Me.AliasTerminator, Me.Alias(a), StringComparison.InvariantCultureIgnoreCase)
        Next

        If (Not String.IsNullOrWhiteSpace(jobName)) Then
            pout = pout.Replace(Me.AliasStarter & ALIAS_JOBNAME & Me.AliasTerminator, jobName, StringComparison.InvariantCultureIgnoreCase)
        End If

        If (Not String.IsNullOrWhiteSpace(taskName)) Then
            pout = pout.Replace(Me.AliasStarter & ALIAS_TASKNAME & Me.AliasTerminator, taskName, StringComparison.InvariantCultureIgnoreCase)
        End If

        If (iMaxThreads > 0) Then
            pout = pout.Replace(Me.AliasStarter & ALIAS_MAXTHREADS & Me.AliasTerminator, CStr(iMaxThreads), StringComparison.InvariantCultureIgnoreCase)
        End If

        Return pout

    End Function

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Check if a given path can be expanded and all aliases are accounted for.
    ''' </summary>
    ''' <param name="pin"></param>
    ''' <returns>True all aliases in the path can be expanded, without leaving any residuals.</returns>
    ''' -----------------------------------------------------------------------
    Public Function CanExpand(pin As String) As Boolean
        ' Expand providing a bogus name for a job and task, just to make sure that the expand command does not generate new alias placeholders.
        Dim test As String = Me.Expand(pin, "job", "task", -1)
        Return ((test.IndexOf(Me.AliasStarter) = -1) And (test.IndexOf(Me.AliasTerminator) = -1))
    End Function

#Region " Settings "

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the string sequence that indicates the start of an alias in a path.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' For example, opener '[' and aliases 'Rpath' and 'Rexe' would be 
    ''' used like 'C:\Program files\[Rpath]\[Rexe]'.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property AliasStarter As String = "["

    ''' -----------------------------------------------------------------------
    ''' <summary>
    ''' Get/set the string sequence that indicates the end of an alias in a path.
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks>
    ''' For example, terminator ']' and aliases 'Rpath' and 'Rexe' would be 
    ''' used like 'C:\Program files\[Rpath]\[Rexe]'.
    ''' </remarks>
    ''' -----------------------------------------------------------------------
    Public Property AliasTerminator As String = "]"

#End Region ' Settings

#Region " Configuration persistence "

    Public Overrides Function Load(settings As cXMLSettings, section As String) As Boolean
        If MyBase.Load(settings, section) Then
            Me.AliasStarter = settings.ReadSetting(section, "AliasStarter", "[")
            Me.AliasTerminator = settings.ReadSetting(section, "AliasTerminator", "]")
            Return True
        End If
        Return False
    End Function

    Public Overrides Function Save(settings As cXMLSettings, section As String) As Boolean
        If MyBase.Save(settings, section) Then
            settings.WriteSetting(section, "AliasStarter", Me.AliasStarter)
            settings.WriteSetting(section, "AliasTerminator", Me.AliasTerminator)
            Return True
        End If
        Return False
    End Function

#End Region ' Configuration persistence

End Class
