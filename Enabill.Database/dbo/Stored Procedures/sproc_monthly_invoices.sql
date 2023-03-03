CREATE PROCEDURE [dbo].[sproc_monthly_invoices]

AS

BEGIN

      -- ======================================================================================================
      -- Procedure Name             :     [dbo].[sproc_monthly_invoices]
      -- Description                :     This SP is used for reporting the month of month client invoiced amount
      -- Current Version            :     1
      -- Created date               :     19-September-2013
      -- Author                     :     Sedick Clarke (Alacrity)
      -- Source Tables/Views        :     invoicerules, invoices, projects, billingmethods
      -- Destination Table/Report   :
      -- Input Parameters           :     InvoicedDate
      -- Output parameters          :
      -- EXEC dbo.sproc_PROCNAME 'Sep 2 2013 12:00:00:000AM'
      -- =======================================================================================================
      /*

      MODIFICATION HISTORY

      VERSION           DATE              USER               DESCRIPTION
      1.0               19/09/2013        SCLARKE            Developed stored procedure for RS

      */

      SET NOCOUNT OFF;

                -- Explanation goes here
                -- ---------------------
                SELECT
                        i.clientname as [Client Name],
                        i.invoiceamountexclvat as [Invoiced Amount],
                        CONVERT(VARCHAR(10),i.invoicedate,111) as [Invoiced Date],
                        b.billingmethodname as [Billing Type]
                FROM
                        invoicerules ir INNER JOIN
                        invoices i ON ir.clientid = i.clientid INNER JOIN
                        projects p ON ir.projectid = p.projectid INNER JOIN
                        billingmethods b ON p.billingmethodid = b.billingmethodid
                GROUP BY
                        i.clientname,
                        i.invoiceamountexclvat,
                        i.invoicedate,
                        b.billingmethodname
                ORDER BY
                        i.clientname

      SET NOCOUNT ON;

END