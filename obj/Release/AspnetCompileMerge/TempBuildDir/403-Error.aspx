<%@ Page Title="403 Forbidden" Language="C#" MasterPageFile="~/ErrorMaster.Master" AutoEventWireup="true" CodeBehind="403-Error.aspx.cs" Inherits="IEGen._403_Error" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ErrorContent" runat="server">
    <div class="container">
        <div class="jumbotron">
            <h2 class="text-danger"><i class="fa fa-exclamation-triangle" aria-hidden="true"></i> <span style="color: #222">403 Forbidden</span></h2>

            <p class="lead">Unfortunately, You don't have access permissions to the page you requested.</p>

            <div class="clearfix" style="padding-top:10px;"></div>

            <span style="font-size:smaller;"><a style="cursor:pointer;" onclick="javascript:history.back();">Go back</a> and try again.</span>

            <div class="clearfix" style="padding-top:20px;"></div>

            <p><a class="btn btn-success btn-lg" runat="server" href="~/Home/Index">Take Me to the Homepage</a></p>
        </div>
    </div>
</asp:Content>
