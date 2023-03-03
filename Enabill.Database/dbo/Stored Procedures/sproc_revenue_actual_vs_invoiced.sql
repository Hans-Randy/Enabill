CREATE PROCEDURE [dbo].[sproc_revenue_actual_vs_invoiced]
(
      @InvoicedDate  datetime = NULL
)

AS

BEGIN

      -- ======================================================================================================
      -- Procedure Name             :     [dbo].[sproc_PROCNAME]
      -- Description                :     This SP is used for reporting the difference between the actual amount and the invoiced amount
      -- Current Version            :     1
      -- Created date               :     17-September-2013
      -- Author                     :     Sedick Clarke (Alacrity)
      -- Source Tables/Views        :     invoicerules, invoices, projects, workallocations, billingmethods
      -- Destination Table/Report   :
      -- Input Parameters           :     InvoicedDate
      -- Output parameters          :
      -- EXEC dbo.sproc_PROCNAME 'Sep 2 2013 12:00:00:000AM'
      -- =======================================================================================================
      /*

      MODIFICATION HISTORY

      VERSION           DATE              USER               DESCRIPTION
      1.0               17/09/2013        SCLARKE            Developed stored procedure for RS

      */

      SET NOCOUNT OFF;

                -- Explanation goes here
                -- ---------------------
                SELECT
                        i.clientname as [Client Name],
                        i.invoiceid as [Invoice Number],
                        sum(w.hoursworked * w.hourlyrate) as [Actual Amount],
                        i.invoiceamountexclvat as [Invoiced Amount],
                        CONVERT(VARCHAR(10),i.invoicedate,111) as [Invoiced Date],
                        b.billingmethodname as [Billing Type]
                FROM
                        invoicerules ir INNER JOIN
                        invoices i ON ir.clientid = i.clientid INNER JOIN
                        projects p ON ir.projectid = p.projectid INNER JOIN
                        workallocations w ON i.invoiceid = w.invoiceid INNER JOIN
                        billingmethods b ON p.billingmethodid = b.billingmethodid
                WHERE
                        i.invoicedate > @InvoicedDate and i.invoicedate < getdate()
                GROUP BY
                        i.clientname,
                        i.invoiceid,
                        i.invoiceamountexclvat,
                        i.invoicedate,
                        b.billingmethodname
                ORDER BY
                        i.clientname,
                        i.invoiceid


      SET NOCOUNT ON;

END