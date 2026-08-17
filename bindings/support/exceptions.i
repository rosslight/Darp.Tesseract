/* Common exception translation for every generated Tesseract entry point. */
%exception
{
  try
  {
    $action
  }
  catch (const std::invalid_argument& exception)
  {
    SWIG_CSharpSetPendingExceptionArgument(SWIG_CSharpArgumentException, exception.what(), "");
    return $null;
  }
  catch (const std::out_of_range& exception)
  {
    SWIG_CSharpSetPendingException(SWIG_CSharpIndexOutOfRangeException, exception.what());
    return $null;
  }
  catch (const std::exception& exception)
  {
    SWIG_CSharpSetPendingException(SWIG_CSharpApplicationException, exception.what());
    return $null;
  }
}
